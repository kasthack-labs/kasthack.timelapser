// #define PERF

namespace kasthack.TimeLapser.Recording.Recorder
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    using kasthack.TimeLapser.Recording.Encoding;
    using kasthack.TimeLapser.Recording.Models;
    using kasthack.TimeLapser.Recording.Snappers.Factory;

    using Microsoft.Extensions.Logging;

    using Timer = System.Timers.Timer;

    // legacy recorder. Overcomplicated and buggy
    public record Recorder(
        ISnapperFactory SnapperFactory,
        IOutputStreamProvider OutputStreamProvider,
        ILogger<Recorder> Logger) : IRecorder
    {
        private const double Second = 1000;
        private const double Minute = Second * 60;
        private const int MinimumInterval = 10; // timeouts don't include process switching

        private readonly ManualResetEventSlim stopWaiter = new();
        private Stopwatch stopwatch;

        public bool Recording { get; private set; } = false;

        public void Start(RecordSettings settings)
        {
            if (this.Recording)
            {
                this.Logger.LogError("Tried to start recording twice");
                throw new InvalidOperationException("Recording is already started");
            }

            this.Logger.LogInformation("Starting recording");
            this.Recording = true;
            _ = Task.Factory.StartNew(async () => await this.StartInternal(settings).ConfigureAwait(false), TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        public void Stop()
        {
            this.Logger.LogInformation("Stopping recording");
            this.Recording = false;
            this.stopWaiter.Wait();
        }

        private void TryIncreaseCurrentThreadsPriorityToRealtime()
        {
            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
                this.Logger.LogDebug("Increased thread priority to {threadPriority}", Thread.CurrentThread.Priority);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Failed to increase thread priority");
            }
        }
        private async Task StartInternal(RecordSettings settings)
        {
            try
            {
                this.Logger.LogDebug("Starting recording(internal)");
                this.TryIncreaseCurrentThreadsPriorityToRealtime();

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
                        this.Logger.LogDebug("Starting new video file");

                        // order matters!
                        using (var snapper = this.SnapperFactory.GetSnapper(settings.SnapperType))
                        using (var outstream = this.OutputStreamProvider.GetOutputStream(settings))
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
                                this.Logger.LogTrace("Entering capture loop");
                                Task delayBetweenFramesTask = null;
                                Bitmap currentFrame = null;
                                try
                                {
                                    framesWritten++;

                                    this.Logger.LogTrace("Frame increment: {frames}", framesWritten);

                                    // drop frame if required
                                    if (dropNextNFrames > 0 && currentFrame != null)
                                    {
                                        this.Logger.LogTrace("Drop frames encountered. dropFrames: {dropFrames} currentFrame {frames}", dropNextNFrames, framesWritten);
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

                                    this.Logger.LogTrace("Requesting a frame from snapper");
                                    var tmp = await snapper.Snap(inputSnapIntervalMilliseconds).ConfigureAwait(false);
                                    var elapsedAfterCurrentSnap = this.stopwatch.Elapsed.TotalMilliseconds;
                                    var snapTime = elapsedAfterCurrentSnap - elapsedBeforeCurrentSnap;

                                    this.Logger.LogTrace("Received a frame in {elapsed} ms", snapTime);

                                    if (snapTime > inputSnapIntervalMilliseconds)
                                    {
                                        slowFramewsSinceLastSync++;
                                        this.Logger.LogTrace("Another slow frame. Slow frames since last sync: {slowFrames}", slowFramewsSinceLastSync);
                                    }

                                    if (tmp == null)
                                    {
                                        emptyFramesSinceLastSync++;
                                        this.Logger.LogTrace("Another empty frame. Empty frames since last sync: {emptyFrames}", emptyFramesSinceLastSync);
                                    }

                                    currentFrame = tmp ?? currentFrame; // settings.OnFrameWritten?.Invoke(_stopwatch.Elapsed);
                                }
                                catch (Exception)
                                {
                                    this.Logger.LogError("Failed to get a frame from snapper");
                                    crashedFramesSinceLastSync++;
                                }

                                _ = Task.Run(async () =>
                                {
                                    this.Logger.LogTrace("Launching preprocessing / writing a frame");
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
                                                this.Logger.LogTrace("Wrote frame to the output file");
                                            }
                                            finally
                                            {
                                                writeSemaphore.Release();
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        this.Logger.LogError(ex, "Encountered an error while processing a frame");
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
                                        var delay = (int)((inputSnapIntervalMilliseconds * recentFpsDelta) - MinimumInterval);
                                        await Task.Delay(delay).ConfigureAwait(false);
                                        this.Logger.LogInformation("Speeding, waiting for time to catch up for {delay} ms", delay);
                                    }
                                    else if (recentFpsDelta < -1)
                                    {
                                        dropNextNFrames = -(int)recentFpsDelta;
                                        this.Logger.LogInformation("Lagging behind, skipping next {frames} frames", dropNextNFrames);
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
                    this.Logger.LogDebug("Exiting recording");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while recoring");
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
                gr.Flush();
            }
        }
    }
}
