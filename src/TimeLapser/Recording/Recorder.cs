﻿// #define PERF
namespace kasthack.TimeLapser
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using Accord.Video.FFMPEG;

    using Timer = System.Timers.Timer;

    public class Recorder
    {
        private const double Second = 1000;
        private const double Minute = Second * 60;
        private const int MinimumInterval = 10; // timeouts don't include process switching

        private readonly ManualResetEventSlim stopWaiter = new();
        private Stopwatch stopwatch;

        public event EventHandler FrameWritten;

        public bool Recording { get; private set; } = false;

        public void Start(RecordSettings settings)
        {
            var timer = new Timer();
            if (this.Recording)
            {
                throw new InvalidOperationException("Recording is already started");
            }

            this.Recording = true;
            _ = Task.Factory.StartNew(async () => await this.StartInternal(settings).ConfigureAwait(false), TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        public void Stop()
        {
            this.Recording = false;
            this.stopWaiter.Wait();
        }

        private static ISnapper GetSnapper(RecordSettings settings) => settings.SnapperType switch
        {
            SnapperType.DirectX => new DXSnapper(),
            SnapperType.Legacy => new SDGSnapper(),
            _ => throw new ArgumentOutOfRangeException($"Invalid snapper: {settings.SnapperType}"),
        };

        private static void TryIncreaseCurrentThreadsPriorityToRealtime()
        {
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }
            catch
            {
            }
        }

        private static VideoFileWriter GetOutputStream(RecordSettings settings)
        {
            if (!Directory.Exists(settings.OutputPath))
            {
                Directory.CreateDirectory(settings.OutputPath);
            }

            var outputFileName = $"timelapser-capture-{DateTimeOffset.Now:yyyy-MM-dd_HH-mm}.avi";
            var outfile = Path.Combine(settings.OutputPath, outputFileName);
            var outstream = new VideoFileWriter();
            outstream.Open(outfile, settings.CaptureRectangle.Width, settings.CaptureRectangle.Height, settings.Fps, settings.Codec, settings.Bitrate);
            return outstream;
        }

        private async Task StartInternal(RecordSettings settings)
        {
            try
            {
                TryIncreaseCurrentThreadsPriorityToRealtime();

                this.stopwatch ??= new Stopwatch();
                this.stopwatch.Reset();
                this.stopwatch.Start();

                using (var updateStatsTimer = this.CreateStatsTimer(settings))
                {
                    var inputSnapIntervalMilliseconds = (int)Math.Max(settings.Realtime ? Second / settings.Fps : settings.Interval, 0); // snap every N ms
                    var splitIntervalInFrames = settings.SplitInterval * Minute / inputSnapIntervalMilliseconds; // split every N frames
                    var inputExpectedFps = inputSnapIntervalMilliseconds > 0 ? Second / inputSnapIntervalMilliseconds : 0;

                    var framesWritten = 0L;
                    while (this.Recording)
                    {
                        // order matters!
                        using (var snapper = GetSnapper(settings))
                        using (var outstream = GetOutputStream(settings))
                        {
                            using var processingSemaphore = new SemaphoreSlim(snapper.MaxProcessingThreads);
                            using var writeSemaphore = new SemaphoreSlim(1);

                            snapper.SetSource(settings.CaptureRectangle);
                            var dropNextNFrames = 0;
                            var lastSyncFrames = framesWritten;
                            double lastSyncTime = this.stopwatch.ElapsedMilliseconds;
                            var emptyFramesSinceLastSync = 0;
                            var crashedFramesSinceLastSync = 0;
                            var slowFramewsSinceLastSync = 0;

                            for (var i = 0L; (splitIntervalInFrames == null || i < splitIntervalInFrames) && this.Recording; i++)
                            {
                                Task delayBetweenFramesTask = null;
                                Bitmap currentFrame = null;
                                try
                                {
                                    framesWritten++;

                                    // drop frame if required
                                    if (dropNextNFrames > 0 && currentFrame != null)
                                    {
                                        dropNextNFrames--;
                                        lastSyncTime = this.stopwatch.ElapsedMilliseconds;
                                        lastSyncFrames = framesWritten;
                                        outstream.WriteVideoFrame(currentFrame);
                                        continue;
                                    }

                                    delayBetweenFramesTask = Task.Delay(inputSnapIntervalMilliseconds - MinimumInterval);
                                    /*
                                    * these bitmaps are actually the same object or null -> we only have to dispose it once
                                    */
                                    var elapsedBeforeCurrentSnap = this.stopwatch.Elapsed.TotalMilliseconds;
                                    await processingSemaphore.WaitAsync().ConfigureAwait(false);
                                    var tmp = await snapper.Snap(inputSnapIntervalMilliseconds).ConfigureAwait(false);
                                    var elapsedAfterCurrentSnap = this.stopwatch.Elapsed.TotalMilliseconds;
                                    if (elapsedAfterCurrentSnap - elapsedBeforeCurrentSnap > inputSnapIntervalMilliseconds)
                                    {
                                        slowFramewsSinceLastSync++;
                                    }

                                    if (tmp == null)
                                    {
                                        emptyFramesSinceLastSync++;
                                    }

                                    currentFrame = tmp ?? currentFrame; // settings.OnFrameWritten?.Invoke(_stopwatch.Elapsed);
                                }
                                catch (Exception)
                                {
                                    crashedFramesSinceLastSync++;
                                }

                                _ = Task.Run(async () =>
                                {
                                    try
                                    {
                                        if (currentFrame != null)
                                        {
                                            // offload to separate thread
                                            this.PreprocessFrame(currentFrame, settings);
                                            await writeSemaphore.WaitAsync().ConfigureAwait(false);
                                            try
                                            {
                                                outstream.WriteVideoFrame(currentFrame);
                                            }
                                            finally
                                            {
                                                writeSemaphore.Release();
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        processingSemaphore.Release();
                                    }
                                }).ConfigureAwait(false);

                                double elapsedNow = this.stopwatch.ElapsedMilliseconds;
                                var elapsedSinceLastSync = elapsedNow - lastSyncTime;
                                if (elapsedSinceLastSync >= Second)
                                {
                                    var framesSinceLastSync = framesWritten - lastSyncFrames;

                                    // only relevant for realtime+ recordings
                                    var recentFps = framesSinceLastSync * Second / elapsedSinceLastSync;
                                    var recentFpsDelta = recentFps - inputExpectedFps;
#if !PERF
                                    // faster than expected && at least one actual frame
                                    if (recentFpsDelta > 1)
                                    {
                                        await delayBetweenFramesTask.ConfigureAwait(false); // wait for the current loop
                                        await Task.Delay((int)((inputSnapIntervalMilliseconds * recentFpsDelta) - MinimumInterval)).ConfigureAwait(false);
                                    }
                                    else if (recentFpsDelta < -1)
                                    {
                                        dropNextNFrames = -(int)recentFpsDelta;
                                    }
#endif
                                    lastSyncFrames = framesWritten;
                                    lastSyncTime = elapsedNow;
                                    (emptyFramesSinceLastSync, crashedFramesSinceLastSync, slowFramewsSinceLastSync) = (0, 0, 0);
                                }
#if !PERF
                                if (delayBetweenFramesTask is not null)
                                {
                                    await delayBetweenFramesTask.ConfigureAwait(false);
                                }
#endif
                            }

                            await writeSemaphore.WaitAsync().ConfigureAwait(false);
                        }
                    }

                    updateStatsTimer.Stop();
                }
            }
            catch
            {
            }
            finally
            {
                this.stopwatch?.Stop();
                this.stopWaiter.Set();
            }
        }

        private Timer CreateStatsTimer(RecordSettings settings)
        {
            var result = new Timer
            {
                Interval = 100,
            };
            result.Elapsed += (a, b) => settings.OnFrameWritten(this.stopwatch.Elapsed);
            result.Start();
            return result;
        }

        private void PreprocessFrame(Bitmap bmp, RecordSettings settings)
        {
            if (settings.Private)
            {
                using var gr = Graphics.FromImage(bmp);
                gr.Clear(Color.Black);
            }
        }
    }
}
