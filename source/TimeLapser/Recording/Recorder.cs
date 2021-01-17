//#define PERF
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using Accord.Video.FFMPEG;

using Timer = System.Timers.Timer;

namespace kasthack.TimeLapser
{

    public class Recorder
    {
        public bool Recording { get; private set; } = false;
        private ManualResetEventSlim _stopWaiter = new ManualResetEventSlim();
        private Stopwatch _stopwatch;
        public event EventHandler FrameWritten;

        public void Start(RecordSettings settings)
        {
            var timer = new Timer();
            if (Recording)
            {
                throw new InvalidOperationException("Recording is already started");
            }

            Recording = true;
            Task.Factory.StartNew(async () => await StartInternal(settings).ConfigureAwait(false), TaskCreationOptions.LongRunning).ConfigureAwait(false);
        }

        private async Task StartInternal(RecordSettings settings)
        {
            try
            {
                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.Highest;
                }
                catch (Exception ex) { }
                _stopwatch = _stopwatch ?? new Stopwatch();
                _stopwatch.Reset();
                _stopwatch.Start();
                _stopWaiter.Reset();

                if (!Directory.Exists(settings.OutputPath))
                {
                    Directory.CreateDirectory(settings.OutputPath);
                }

                const double second = 1000;
                const double minute = second * 60;
                const int quant = 10;//timeouts don't include process switching
                var inputSnapInterval = (int)Math.Max((settings.Realtime ? second / settings.Fps : settings.Interval), 0);//snap every N ms
                var splitInterval = settings.SplitInterval * minute / inputSnapInterval;//split every N frames
                var inputExpectedFps = inputSnapInterval > 0 ? second / inputSnapInterval : 0;
                var sourceRect = settings.CaptureRectangle;
                
                var framesWritten = 0L;
                using (var tmr = new Timer())
                {
                    tmr.Interval = 100;
                    tmr.Elapsed += (a, b) => settings.OnFrameWritten(_stopwatch.Elapsed);
                    tmr.Start();
                    while (Recording)
                    {
                        var outfile = Path.Combine(settings.OutputPath, DateTime.Now.ToFileTime().ToString() + ".avi");
                        using (var outstream = new VideoFileWriter())
                        {
                            outstream.Open(outfile, sourceRect.Width, sourceRect.Height, settings.Fps, settings.Codec, settings.Bitrate);
                            using (ISnapper snapper = GetSnapper(settings))
                            {
                                using (var processingSemaphore = new SemaphoreSlim(snapper.MaxProcessingThreads))
                                {
                                    using (var writeSemaphore = new SemaphoreSlim(1))
                                    {
                                        snapper.SetSource(sourceRect);
                                        var dropNextNFrames = 0;
                                        var lastSyncFrames = framesWritten;
                                        double lastSyncTime = _stopwatch.ElapsedMilliseconds;
                                        var emptyFramesSinceLastSync = 0;
                                        var crashedFramesSinceLastSync = 0;
                                        var slowFramewsSinceLastSync = 0;

                                        for (var i = 0L; (splitInterval == null || i < splitInterval) && Recording; i++)
                                        {
                                            Task tsk = null;
                                            Bitmap currentFrame = null;
                                            try
                                            {
                                                framesWritten++;

                                                //drop frame if required
                                                if (dropNextNFrames > 0 && currentFrame != null)
                                                {
                                                    dropNextNFrames--;
                                                    lastSyncTime = _stopwatch.ElapsedMilliseconds;
                                                    lastSyncFrames = framesWritten;
                                                    outstream.WriteVideoFrame(currentFrame);
                                                    continue;
                                                }
                                                tsk = Task.Delay(inputSnapInterval - quant);
                                                /*
                                                    * these bitmaps are actually the same object or null -> we only have to dispose it once
                                                    */
                                                var elapsedBeforeCurrentSnap = _stopwatch.Elapsed.TotalMilliseconds;
                                                await processingSemaphore.WaitAsync().ConfigureAwait(false);
                                                var tmp = await snapper.Snap(inputSnapInterval).ConfigureAwait(false);
                                                var elapsedAfterCurrentSnap = _stopwatch.Elapsed.TotalMilliseconds;
                                                if (elapsedAfterCurrentSnap - elapsedBeforeCurrentSnap > inputSnapInterval)
                                                {
                                                    //Debug.WriteLine($"[FUCK] slow snap: {elapsedAfterCurrentSnap - elapsedBeforeCurrentSnap}ms");
                                                    slowFramewsSinceLastSync++;
                                                }
                                                if (tmp == null)
                                                {
                                                    //Debug.WriteLine("[FUCK] empty snap");
                                                    emptyFramesSinceLastSync++;
                                                }
                                                currentFrame = tmp ?? currentFrame;
                                                //Debug.WriteLine($"[SNAP] {_stopwatch.ElapsedMilliseconds} ms");
                                                //settings.OnFrameWritten?.Invoke(_stopwatch.Elapsed);
                                            }
                                            catch (Exception ex)
                                            {
                                                //Debug.WriteLine($"[FUCK] crashed on snap: {ex.Message}");
                                                crashedFramesSinceLastSync++;
                                            }
                                            try
                                            {
                                                Task.Run(async () =>
                                                {
                                                    try
                                                    {
                                                        if (currentFrame != null)
                                                        {//offload to separate thread

                                                            PreprocessFrame(currentFrame, settings);
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
                                            catch (Exception ex)
                                            {
                                                //todo: crashed frame logging
                                            }

                                            double elapsedNow = _stopwatch.ElapsedMilliseconds;
                                            var elapsedSinceLastSync = elapsedNow - lastSyncTime;
                                            if (elapsedSinceLastSync >= second)
                                            {
                                                var framesSinceLastSync = framesWritten - lastSyncFrames;
                                                //only relevant for realtime+ recordings
                                                var recentFps = framesSinceLastSync * second / elapsedSinceLastSync;
                                                var recentFpsDelta = recentFps - inputExpectedFps;
                                                Console.WriteLine($"{framesSinceLastSync} frames({emptyFramesSinceLastSync} empty, {crashedFramesSinceLastSync} crashed, {slowFramewsSinceLastSync} slow) in last {elapsedSinceLastSync.ToString("F")} ms ({recentFps.ToString("F")} fps). Total FPS: {(framesWritten * second / elapsedNow).ToString("F")}. Expected: {inputExpectedFps.ToString("F")}");
#if !PERF
                                                if (recentFpsDelta > 1) //faster than expected && at least one actual frame
                                                {
                                                    tsk.Wait(); //wait for the current loop
                                                                //Debug.WriteLine($"[FUCK] Slow down, fella: {recentFpsDelta} frames");
                                                    Task.Delay((int)(inputSnapInterval * recentFpsDelta - quant)).Wait();
                                                }
                                                else if (recentFpsDelta < -1)//too slow
                                                {
                                                    dropNextNFrames = -(int)recentFpsDelta;
                                                    //Debug.WriteLine($"[FUCK] dropping {dropNextNFrames} frames");
                                                }
#endif
                                                lastSyncFrames = framesWritten;
                                                lastSyncTime = elapsedNow;
                                                emptyFramesSinceLastSync = 0;
                                                crashedFramesSinceLastSync = 0;
                                                slowFramewsSinceLastSync = 0;
                                            }
#if !PERF
                                            tsk?.Wait();
#endif
                                        }
                                        await writeSemaphore.WaitAsync().ConfigureAwait(false);
                                    }
                                }
                            }
                        }
                    }
                    tmr.Stop();
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(ex.Message);
                //global
            }
            finally
            {
                _stopwatch?.Stop();
                _stopWaiter.Set();
            }
        }

        private static ISnapper GetSnapper(RecordSettings settings)
        {
            switch (settings.SnapperType)
            {
                case SnapperType.DirectX: return new DXSnapper();
                case SnapperType.Legacy: return new SDGSnapper();
                default: throw new ArgumentOutOfRangeException($"Invalid snapper: {settings.SnapperType}");
            }
        }

        private void PreprocessFrame(Bitmap bmp, RecordSettings settings)
        {
            if (settings.Private)
            {
                using (var gr = Graphics.FromImage(bmp))
                {
                    gr.Clear(Color.Black);
                }
            }
        }

        public void Stop()
        {
            Recording = false;
            _stopWaiter.Wait();
        }
    }
}