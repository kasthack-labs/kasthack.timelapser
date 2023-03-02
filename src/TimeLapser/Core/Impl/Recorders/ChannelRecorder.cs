namespace kasthack.TimeLapser.Core.Impl.Recorders;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using kasthack.TimeLapser.Core.Interfaces;
using kasthack.TimeLapser.Core.Model;
using kasthack.TimeLapser.Core.Models;

using Microsoft.Extensions.Logging;

// new recorder implementation using channels
internal record ChannelRecorder(
    ISnapperFactory SnapperFactory,
    IOutputVideoStreamProvider OutputStreamProvider,
    ILogger<Recorder> Logger) : IRecorder
{
    /* if we need to have more than 2 frames queued up, there's something very wrong with the snapper anyway,
        capture timings are incredibly inconsistent, and timeouts are outright broken */
    private const int MaxParallelCaptureTasks = 2;
    private CancellationTokenSource cts;
    private readonly ManualResetEventSlim stopWaiter = new();

    ~ChannelRecorder()
    {
        if (this.Recording)
        {
            this.Stop();
        }
    }

    public bool Recording => this.cts is not null && !this.cts.IsCancellationRequested;

    /// <inheritdoc/>
    public void Start(RecordSettings settings)
    {
        if (this.Recording)
        {
            throw new InvalidOperationException("Already recording!");
        }

        this.cts = new CancellationTokenSource();
        this.Logger.LogInformation("Starting recording, settings: {settings}", settings);
        this.stopWaiter.Reset();
        _ = Task.Run(async () => await this.StartInternal(settings).ConfigureAwait(false)).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public void Stop()
    {
        if (!this.Recording)
        {
            throw new InvalidOperationException("Already stopped");
        }

        this.Logger.LogCritical("Received STOP signal for recording");
        this.cts.Cancel();
        this.stopWaiter.Wait();
        this.cts.Dispose();
        this.cts = null;
        this.Logger.LogCritical("Stopped recording");
    }

    private async Task StartInternal(RecordSettings settings)
    {
        this.Logger.LogDebug("Starting internal recording task");
        using var snapper = this.SnapperFactory.GetSnapper(settings.SnapperType);
        snapper.SetSource(settings.CaptureRectangle);

        var captureChannel = Channel.CreateBounded<OrderedFrame>(snapper.MaxProcessingThreads);
        var reorderChannel = Channel.CreateBounded<OrderedFrame>(snapper.MaxProcessingThreads);
        var disposeChannel = Channel.CreateBounded<OrderedFrame>(snapper.MaxProcessingThreads);

        this.Logger.LogDebug("Starting pipelines");

        var captureTask = Task.Run(async () => await this.CaptureLoop(snapper, captureChannel.Writer, settings).ConfigureAwait(false));
        var reorderTask = Task.Run(async () => await this.ReorderLoop(captureChannel.Reader, reorderChannel.Writer, MaxParallelCaptureTasks).ConfigureAwait(false));
        var encodeTask = Task.Run(async () => await this.EncodeLoop(reorderChannel.Reader, disposeChannel.Writer, settings).ConfigureAwait(false));
        var disposeTask = Task.Run(async () => await this.DisposeLoop(disposeChannel.Reader).ConfigureAwait(false));

        try
        {
            await Task.WhenAll(captureTask, reorderTask, encodeTask, disposeTask).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error in capture or processing loop");
        }
        finally
        {
            this.stopWaiter.Set();
            this.Logger.LogError("Stopped internal recording task");
        }
    }

    private async Task DisposeLoop(ChannelReader<OrderedFrame> reader)
    {
        this.Logger.LogDebug("Dispose loop started");
        try
        {
            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out var frame))
                {
                    try
                    {
                        this.Logger.LogTrace("Releasing frame {frameId}", frame.FrameId);
                        frame.Frame.Dispose();
                    }
                    catch (Exception ex)
                    {
                        this.Logger.LogError(ex, "Failed to dispose frame {frameId}", frame.FrameId);
                    }
                }
            }
        }
        finally
        {
            this.Logger.LogError("Dispose loop stopped");
        }
    }

    /// <summary>
    /// capture frames from snapper, write to channelWriter.
    /// </summary>
    /// <param name="snapper">Snapper to use.</param>
    /// <param name="writer">Output writer.</param>
    /// <param name="settings">Capture settings.</param>
    /// <returns>Awaitable task.</returns>
    private async Task CaptureLoop(ISnapper snapper, ChannelWriter<OrderedFrame> writer, RecordSettings settings)
    {
        this.Logger.LogDebug("Capture loop started");
        var frameId = 0L;
        using var countdown = new CountdownEvent(1);
        using var captureSemaphore = new SemaphoreSlim(MaxParallelCaptureTasks);
        try
        {
            while (this.Recording)
            {
                var delay = settings.Realtime ? 1000 / settings.Fps : settings.Interval;
                var delayTask = Task.Delay(delay, this.cts.Token);
                var currentFrameId = ++frameId;

                this.Logger.LogTrace("Incrementing countdown and acquiring semaphore for capturing frame {frameId}", currentFrameId);
                countdown.AddCount();
                await captureSemaphore.WaitAsync().ConfigureAwait(false);
                _ = Task.Run(async () =>
                {
                    try
                    {
                        this.Logger.LogTrace("Capturing frame {frameId}", currentFrameId);
                        var bmp = await snapper.Snap(delay).ConfigureAwait(false);
                        if (bmp is not null)
                        {
                            if (writer.TryWrite(new(currentFrameId, bmp)))
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
                        await writer.WriteAsync(new(currentFrameId, null)).ConfigureAwait(false);
                    }
                    finally
                    {
                        _ = captureSemaphore.Release();
                        _ = countdown.Signal();
                    }
                }).ConfigureAwait(false);
                this.Logger.LogTrace("Waiting for the next frame after {frameId}", currentFrameId);

                try
                {
                    await delayTask.ConfigureAwait(false);
                }
                catch
                {
                }
            }
        }
        catch (Exception ex)
        {
            this.Logger.LogError(ex, "Error while running capture loop");
        }
        finally
        {
            this.Logger.LogTrace("Stopping capture loop");
            _ = countdown.Signal();
            countdown.Wait();
            this.Logger.LogTrace("Closing capture channel writer");
            writer.Complete();
            this.Logger.LogError("Capture loop stopped");
        }
    }

    /// <summary>
    /// buffer last n items from channelReader, sort them by frameId, replace nulls with previous snap, write to channelWriter.
    /// </summary>
    /// <param name="reader">Reader to consumer frames from.</param>
    /// <param name="writer">Reader to write sorted frames to.</param>
    /// <param name="bufferSize">Reorder buffer size.</param>
    /// <returns>Awaitable task.</returns>
    private async Task ReorderLoop(ChannelReader<OrderedFrame> reader, ChannelWriter<OrderedFrame> writer, int bufferSize)
    {
        this.Logger.LogDebug("Starting reorder loop");
        try
        {
            var chunk = new List<OrderedFrame>();
            IPooledFrame lastFrame = null;
            while (this.Recording && await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out var item))
                {
                    chunk.Add(item);
                    if (chunk.Count == bufferSize)
                    {
                        ForwardChunk();
                    }
                }
            }

            ForwardChunk();

            void ForwardChunk()
            {
                if (chunk.Count == 0)
                {
                    return;
                }

                chunk.Sort((a, b) => a.FrameId.CompareTo(b.FrameId));
                this.Logger.LogTrace(
                    "Forwarding chunk of {count} frames, first frame id {firstFrameId}, last frame id {lastFrameId}",
                    chunk.Count,
                    chunk[0].FrameId,
                    chunk[^1].FrameId);
                foreach (var (frameId, frame) in chunk)
                {
                    var outputFrame = frame;
                    if (outputFrame is null)
                    {
                        outputFrame = lastFrame;
                    }
                    else
                    {
                        lastFrame = outputFrame;
                    }

                    if (outputFrame is null)
                    {
                        this.Logger.LogWarning("Frame {frameId} is null, skipping", frameId);
                        continue;
                    }

                    if (!writer.TryWrite(new(frameId, outputFrame)))
                    {
                        this.Logger.LogError("Congested reorder writer! Dropping frame {frameId}", frameId);
                    }
                }

                chunk.Clear();
            }
        }
        finally
        {
            this.Logger.LogDebug("Closing reorder channel writer");
            writer.Complete();
            this.Logger.LogError("Reorder loop stopped");
        }
    }

    /// <summary>
    /// read from channelReader, write to video file, split output file as needed.
    /// </summary>
    /// <param name="reader">Reader to consume frames from.</param>
    /// <param name="writer">Writer to send frames for disposal.</param>
    /// <param name="settings">Record settings.</param>
    /// <returns>Awaitable task.</returns>
    private async Task EncodeLoop(ChannelReader<OrderedFrame> reader, ChannelWriter<OrderedFrame> writer, RecordSettings settings)
    {
        this.Logger.LogDebug("Starting encode loop");
        try
        {
            var splitIntervalInFrames = settings.SplitInterval * settings.Fps;
            while (this.Recording)
            {
                using (var outstream = this.OutputStreamProvider.GetOutputStream(settings))
                {
                    while (this.Recording && await reader.WaitToReadAsync().ConfigureAwait(false))
                    {
                        while (reader.TryRead(out var item))
                        {
                            var frame = item.Frame;
                            this.Logger.LogTrace("Writing frame {frameId} to output stream", item.FrameId);
                            try
                            {
                                outstream.WriteVideoFrame(frame.Value);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, "Error writing frame {frameId} to output stream", item.FrameId);
                            }
                            finally
                            {
                                await writer.WriteAsync(item).ConfigureAwait(false);
                            }

                            if (item.FrameId % splitIntervalInFrames == 0)
                            {
                                this.Logger.LogInformation("Splitting output file {file} at frame {frameId}", item.FrameId);
                                goto outer;
                            }
                        }
                    }

#pragma warning disable SA1024 // stylecop has issues with labels
                outer:;
#pragma warning restore SA1024
                }
            }
        }
        finally
        {
            this.Logger.LogDebug("Stopping encode loop");
            writer.Complete();
            this.Logger.LogError("Encode loop stopped");
        }
    }

    private record struct OrderedFrame(long FrameId, IPooledFrame Frame);
}
