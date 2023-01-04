// #define PERF
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

        private async Task StartInternal(RecordSettings settings)
        {
            try
            {
                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                }
                catch (Exception)
                {
                }

                this.stopwatch ??= new Stopwatch();
                this.stopwatch.Reset();
                this.stopwatch.Start();
                this.stopWaiter.Reset();

                if (!Directory.Exists(settings.OutputPath))
                {
                    Directory.CreateDirectory(settings.OutputPath);
                }

                const double second = 1000;
                const double minute = second * 60;
                const int quant = 10; // timeouts don't include process switching
                var inputSnapInterval = (int)Math.Max(settings.Realtime ? second / settings.Fps : settings.Interval, 0); // snap every N ms
                var splitInterval = settings.SplitInterval * minute / inputSnapInterval; // split every N frames
                var inputExpectedFps = inputSnapInterval > 0 ? second / inputSnapInterval : 0;
                var sourceRect = settings.CaptureRectangle;

                var framesWritten = 0L;
                using var tmr = new Timer
                {
                    Interval = 100,
                };
                tmr.Elapsed += (a, b) => settings.OnFrameWritten(this.stopwatch.Elapsed);
                tmr.Start();
                while (this.Recording)
                {
                    var outfile = Path.Combine(settings.OutputPath, DateTime.Now.ToFileTime().ToString() + ".avi");
                    using var outstream = new VideoFileWriter();
                    outstream.Open(outfile, sourceRect.Width, sourceRect.Height, settings.Fps, settings.Codec, settings.Bitrate);

                    using var snapper = GetSnapper(settings);
                    using var processingSemaphore = new SemaphoreSlim(snapper.MaxProcessingThreads);
                    using var writeSemaphore = new SemaphoreSlim(1);

                    snapper.SetSource(sourceRect);
                    var dropNextNFrames = 0;
                    var lastSyncFrames = framesWritten;
                    double lastSyncTime = this.stopwatch.ElapsedMilliseconds;
                    var emptyFramesSinceLastSync = 0;
                    var crashedFramesSinceLastSync = 0;
                    var slowFramewsSinceLastSync = 0;

                    for (var i = 0L; (splitInterval == null || i < splitInterval) && this.Recording; i++)
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

                            delayBetweenFramesTask = Task.Delay(inputSnapInterval - quant);
                            /*
                                * these bitmaps are actually the same object or null -> we only have to dispose it once
                                */
                            var elapsedBeforeCurrentSnap = this.stopwatch.Elapsed.TotalMilliseconds;
                            await processingSemaphore.WaitAsync().ConfigureAwait(false);
                            var tmp = await snapper.Snap(inputSnapInterval).ConfigureAwait(false);
                            var elapsedAfterCurrentSnap = this.stopwatch.Elapsed.TotalMilliseconds;
                            if (elapsedAfterCurrentSnap - elapsedBeforeCurrentSnap > inputSnapInterval)
                            {
                                // Debug.WriteLine($"[FUCK] slow snap: {elapsedAfterCurrentSnap - elapsedBeforeCurrentSnap}ms");
                                slowFramewsSinceLastSync++;
                            }

                            if (tmp == null)
                            {
                                // Debug.WriteLine("[FUCK] empty snap");
                                emptyFramesSinceLastSync++;
                            }

                            currentFrame = tmp ?? currentFrame;

                            // Debug.WriteLine($"[SNAP] {_stopwatch.ElapsedMilliseconds} ms");
                            // settings.OnFrameWritten?.Invoke(_stopwatch.Elapsed);
                        }
                        catch (Exception)
                        {
                            // Debug.WriteLine($"[FUCK] crashed on snap: {ex.Message}");
                            crashedFramesSinceLastSync++;
                        }

                        try
                        {
                            Task.Run(async () =>
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
                        }
                        catch (Exception)
                        {
                            // todo: crashed frame logging
                        }

                        double elapsedNow = this.stopwatch.ElapsedMilliseconds;
                        var elapsedSinceLastSync = elapsedNow - lastSyncTime;
                        if (elapsedSinceLastSync >= second)
                        {
                            var framesSinceLastSync = framesWritten - lastSyncFrames;

                            // only relevant for realtime+ recordings
                            var recentFps = framesSinceLastSync * second / elapsedSinceLastSync;
                            var recentFpsDelta = recentFps - inputExpectedFps;
                            //Console.WriteLine($"{framesSinceLastSync} frames({emptyFramesSinceLastSync} empty, {crashedFramesSinceLastSync} crashed, {slowFramewsSinceLastSync} slow) in last {elapsedSinceLastSync:F} ms ({recentFps:F} fps). Total FPS: {framesWritten * second / elapsedNow:F}. Expected: {inputExpectedFps:F}");
#if !PERF
                            // faster than expected && at least one actual frame
                            if (recentFpsDelta > 1)
                            {
                                await delayBetweenFramesTask.ConfigureAwait(false); // wait for the current loop

                                // Debug.WriteLine($"[FUCK] Slow down, fella: {recentFpsDelta} frames");
                                await Task.Delay((int)((inputSnapInterval * recentFpsDelta) - quant)).ConfigureAwait(false);
                            }
                            else if (recentFpsDelta < -1)
                            {
                                // too slow
                                dropNextNFrames = -(int)recentFpsDelta;

                                // Debug.WriteLine($"[FUCK] dropping {dropNextNFrames} frames");
                            }
#endif
                            lastSyncFrames = framesWritten;
                            lastSyncTime = elapsedNow;
                            emptyFramesSinceLastSync = 0;
                            crashedFramesSinceLastSync = 0;
                            slowFramewsSinceLastSync = 0;
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

                tmr.Stop();
            }
            catch (Exception)
            {
                // Debug.WriteLine(ex.Message);
                // global
            }
            finally
            {
                this.stopwatch?.Stop();
                this.stopWaiter.Set();
            }
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
