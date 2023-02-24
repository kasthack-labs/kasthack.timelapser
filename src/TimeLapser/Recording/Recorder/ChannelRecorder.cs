namespace kasthack.TimeLapser.Recording.Recorder;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using kasthack.TimeLapser.Recording.Encoding;
using kasthack.TimeLapser.Recording.Models;
using kasthack.TimeLapser.Recording.Snappers;
using kasthack.TimeLapser.Recording.Snappers.Factory;

using Microsoft.Extensions.Logging;

// new recorder implementation using channels
internal record ChannelRecorder(
    ISnapperFactory SnapperFactory,
    IOutputStreamProvider OutputStreamProvider,
    ILogger<Recorder> Logger
) : IRecorder
{
    private CancellationTokenSource cts;
    private readonly ManualResetEventSlim stopWaiter = new();

    ~ChannelRecorder()
    {
        if (this.Recording)
        {
            this.Stop();
        }
    }

    public bool Recording
    {
        get => this.cts is not null && !this.cts.IsCancellationRequested;
    }

    public void Start(RecordSettings settings)
    {
        if (this.Recording)
        {
            throw new InvalidOperationException("Already recording!");
        }

        this.cts = new CancellationTokenSource();
        this.Logger.LogInformation("Starting recording");
        this.stopWaiter.Reset();
        _ = Task.Run(async () => await this.StartInternal(settings).ConfigureAwait(false)).ConfigureAwait(false);
    }

    private async Task StartInternal(RecordSettings settings)
    {
        this.Logger.LogDebug("Starting recoding");
        using (var snapper = this.SnapperFactory.GetSnapper(settings.SnapperType))
        {
            snapper.SetSource(settings.CaptureRectangle);

            var captureChannel = Channel.CreateBounded<(long, Bitmap)>(snapper.MaxProcessingThreads);
            var reorderChannel = Channel.CreateBounded<(long, Bitmap)>(snapper.MaxProcessingThreads);

            this.Logger.LogDebug("Starting pipelines");

            const int reorderBufferSize = 2; // if we need to have more than 2 frames queued up, there's something very wrong with the snapper anyway,
                                             // capture timings are incredibly inconsistent, and timeouts are outright broken
            var captureTask = Task.Run(async () => await this.CaptureLoop(snapper, captureChannel.Writer, settings).ConfigureAwait(false));
            var reorderTask = Task.Run(async () => await this.ReorderLoop(captureChannel.Reader, reorderChannel.Writer, reorderBufferSize).ConfigureAwait(false));
            var encodeTask = Task.Run(async () => await this.EncodeLoop(reorderChannel.Reader, settings).ConfigureAwait(false));

            try
            {
                await Task.WhenAll(captureTask, reorderTask, encodeTask).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error in capture or processing loop");
            }
            finally
            {
                this.stopWaiter.Set();
                this.Logger.LogDebug("Stopped recording");
            }
        }
    }

    // capture frames from snapper, write to channelWriter
    private async Task CaptureLoop(ISnapper snapper, ChannelWriter<(long frameId, Bitmap frame)> writer, RecordSettings settings)
    {
        this.Logger.LogDebug("Capture loop started");
        var frameId = 0L;
        var countdown = new CountdownEvent(1);
        try
        {
            while (this.Recording)
            {
                var delay = settings.Realtime ? 1000 / settings.Fps : settings.Interval;
                var delayTask = Task.Delay(delay, this.cts.Token);
                var currentFrameId = ++frameId;
                this.Logger.LogTrace("Incrementing countdown for frame {frameId}", currentFrameId);
                countdown.AddCount();
                _ = Task.Run(async () =>
                {
                    try
                    {
                        this.Logger.LogTrace("Capturing frame {frameId}", currentFrameId);
                        var bmp = await snapper.Snap(delay).ConfigureAwait(false);
                        if (bmp is not null)
                        {
                            if (writer.TryWrite((currentFrameId, bmp)))
                            {
                                this.Logger.LogTrace("Captured frame {frameId}", currentFrameId);
                            }
                            else
                            {
                                this.Logger.LogWarning("Congested capture writer! Dropping frame {frameId}", frameId);
                            }
                        }
                        else
                        {
                            this.Logger.LogWarning("Frame {frameId} is null, skipping", frameId);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Error capturing frame {frameId}, writing null", currentFrameId);
                        await writer.WriteAsync((currentFrameId, null)).ConfigureAwait(false);
                    }
                    finally
                    {
                        countdown.Signal();
                    }
                }).ConfigureAwait(false);
                this.Logger.LogTrace("Waiting for the next frame after {frameId}", currentFrameId);
                await delayTask.ConfigureAwait(false);
            }

            this.Logger.LogDebug("Capture loop stopped");
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error while running capture loop");
        }
        finally
        {
            this.Logger.LogTrace("Stopping capture loop");
            countdown.Signal();
            countdown.Wait();
            this.Logger.LogTrace("Closing capture channel writer");
            writer.Complete();
            this.Logger.LogDebug("Capture loop stopped");
        }

    }

    //buffer last n items from channelReader, sort them by frameId, replace nulls with previous snap, write to channelWriter
    private async Task ReorderLoop(ChannelReader<(long frameId, Bitmap frame)> reader, ChannelWriter<(long intervalId, Bitmap frame)> writer, int bufferSize)
    {
        this.Logger.LogDebug("Starting reorder loop");
        try
        {
            var chunk = new List<(long frameId, Bitmap frame)>();
            Bitmap lastFrame = null;
            while (await reader.WaitToReadAsync().ConfigureAwait(false) && this.Recording)
            {
                while (reader.TryRead(out var item))
                {
                    chunk.Add(item);
                    if (chunk.Count == bufferSize)
                    {
                        await ForwardChunk().ConfigureAwait(false);
                    }
                }
            }

            await ForwardChunk().ConfigureAwait(false);
            async Task ForwardChunk()
            {
                if (chunk.Count == 0)
                {
                    return;
                }

                chunk.Sort((a, b) => a.frameId.CompareTo(b.frameId));
                this.Logger.LogTrace(
                    "Forwarding chunk of {count} frames, first frame id {firstFrameId}, last frame id {lastFrameId}",
                    chunk.Count,
                    chunk[0].frameId,
                    chunk[^1].frameId);
                foreach (var item in chunk)
                {
                    var frame = item.frame;
                    if (frame == null)
                    {
                        frame = lastFrame;
                    }
                    else
                    {
                        lastFrame = frame;
                    }

                    if (!writer.TryWrite((item.frameId, frame)))
                    {
                        this.Logger.LogError("Congested reorder writer! Dropping frame {frameId}", item.frameId);
                    }
                }

                chunk.Clear();
            }
        }
        finally
        {
            this.Logger.LogTrace("Closing reorder channel writer");
            writer.Complete();
        }

        this.Logger.LogDebug("Exiting reorder loop");
    }

    // read from channelReader, write to video file, split output file as needed
    private async Task EncodeLoop(ChannelReader<(long, Bitmap)> reader, RecordSettings settings)
    {
        this.Logger.LogDebug("Starting encode loop");
        var splitIntervalInFrames = settings.SplitInterval * settings.Fps;
        while (this.Recording)
        {
            using (var outstream = this.OutputStreamProvider.GetOutputStream(settings))
            {
                while (await reader.WaitToReadAsync().ConfigureAwait(false) && this.Recording)
                {
                    while (reader.TryRead(out (long frameId, Bitmap frame) item))
                    {
                        var frame = item.frame;
                        this.Logger.LogTrace("Writing frame {frameId} to output stream", item.frameId);
                        try
                        {
                            outstream.WriteVideoFrame(frame);
                        }
                        catch (Exception ex)
                        {
                            this.Logger.LogError(ex, "Error writing frame {frameId} to output stream", item.frameId);
                        }

                        if (item.frameId % splitIntervalInFrames == 0)
                        {
                            this.Logger.LogInformation("Splitting output file {file} at frame {frameId}", item.frameId);
                            goto outer;
                        }
                    }
                }

            outer:;
            }
        }

        this.Logger.LogDebug("Exiting encode loop");
    }

    public void Stop()
    {
        if (!this.Recording)
        {
            throw new InvalidOperationException("Already stopped");
        }

        this.Logger.LogInformation("Stopping recording");
        this.cts.Cancel();
        this.stopWaiter.Wait();
        this.cts.Dispose();
        this.cts = null;
    }
}
