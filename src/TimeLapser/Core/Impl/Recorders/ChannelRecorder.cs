namespace kasthack.TimeLapser.Core.Impl.Recorders;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
    private readonly ManualResetEventSlim investigationEvent = new(true); // pauses all pipelines

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
        var reorderChannel = Channel.CreateBounded<OrderedFrame>(MaxParallelCaptureTasks);
        var disposeChannel = Channel.CreateBounded<OrderedFrame>(snapper.MaxProcessingThreads);

        this.Logger.LogDebug("Starting pipelines");
        try
        {
            await Task.WhenAll(
                new[]
                {
                  () => this.CaptureLoop(snapper, captureChannel.Writer, settings),
                  () => this.ReorderLoop(captureChannel.Reader, reorderChannel.Writer, MaxParallelCaptureTasks),
                  () => this.EncodeLoop(reorderChannel.Reader, disposeChannel.Writer, settings),
                  () => this.DisposeLoop(disposeChannel.Reader),
                }
                .Select(task => Task.Run(async () => await task().ConfigureAwait(false))))
                .ConfigureAwait(false);
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

    private async Task DisposeLoop(ChannelReader<OrderedFrame> reader) => await this.RunPipelineFragment(
        async () =>
        {
            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                this.investigationEvent.Wait();
                while (reader.TryRead(out var frame))
                {
                    this.investigationEvent.Wait();
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
        },
        "dispose").ConfigureAwait(false);

    /// <summary>
    /// capture frames from snapper, write to channelWriter.
    /// </summary>
    /// <param name="snapper">Snapper to use.</param>
    /// <param name="writer">Output writer.</param>
    /// <param name="settings">Capture settings.</param>
    /// <returns>Awaitable task.</returns>
    private async Task CaptureLoop(ISnapper snapper, ChannelWriter<OrderedFrame> writer, RecordSettings settings)
    {
        var frameId = 0L;
        using var countdown = new CountdownEvent(1);
        using var captureSemaphore = new SemaphoreSlim(MaxParallelCaptureTasks);

        await this.RunPipelineFragment(
            async () =>
            {
                while (this.Recording)
                {
                    this.investigationEvent.Wait();
                    var delay = settings.Realtime ? 1000 / settings.Fps : settings.Interval;
                    var delayTask = Task.Delay(delay, this.cts.Token);
                    var currentFrameId = ++frameId;

                    this.Logger.LogTrace("Incrementing countdown and acquiring semaphore for capturing frame {frameId}, current count {activeTaskCountEvent}", currentFrameId, countdown.CurrentCount - 1);
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
                            this.Logger.LogTrace("Released capture semaphore and event. Current tasks: {activeTaskCountSemaphore}(sem), {activeTaskCountEvent}(ev)", captureSemaphore.CurrentCount, countdown.CurrentCount - 1);
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
            },
            "capture",
            async () =>
            {
                _ = countdown.Signal();
                if (countdown.CurrentCount != 0)
                {
                    this.Logger.LogTrace("Waiting for capture countdown, current value: {staleCaptureTaskCount}", countdown.CurrentCount);
                    countdown.Wait();
                    this.Logger.LogTrace("Completed waiting for stale caputre tasks");
                }
                this.Logger.LogTrace("Closing capture channel writer");
            },
            writer).ConfigureAwait(false);
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
        var chunk = new List<OrderedFrame>();
        IPooledFrame lastFrame = null;

        await this.RunPipelineFragment(
            async () =>
            {
                while (this.Recording && await reader.WaitToReadAsync().ConfigureAwait(false))
                {
                    this.investigationEvent.Wait();
                    while (reader.TryRead(out var item))
                    {
                        this.investigationEvent.Wait();
                        chunk.Add(item);
                        if (chunk.Count == bufferSize)
                        {
                            ForwardChunk();
                        }
                    }
                }

                ForwardChunk();
            },
            "reorder",
            writer: writer)
            .ConfigureAwait(false);
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

    /// <summary>
    /// read from channelReader, write to video file, split output file as needed.
    /// </summary>
    /// <param name="reader">Reader to consume frames from.</param>
    /// <param name="writer">Writer to send frames for disposal.</param>
    /// <param name="settings">Record settings.</param>
    /// <returns>Awaitable task.</returns>
    private async Task EncodeLoop(ChannelReader<OrderedFrame> reader, ChannelWriter<OrderedFrame> writer, RecordSettings settings) => await this.RunPipelineFragment(
        async () =>
        {
            var splitIntervalInFrames = settings.SplitInterval * settings.Fps;
            while (this.Recording)
            {
                this.investigationEvent.Wait();
                using (var outstream = this.OutputStreamProvider.GetOutputStream(settings))
                {
                    while (this.Recording && await reader.WaitToReadAsync().ConfigureAwait(false))
                    {
                        this.investigationEvent.Wait();
                        while (reader.TryRead(out var currentFrame))
                        {
                            this.investigationEvent.Wait();
                            var frame = currentFrame.Frame;
                            this.Logger.LogTrace("Writing frame {frameId} to output stream", currentFrame.FrameId);
                            try
                            {
                                outstream.WriteVideoFrame(frame.Value);
                            }
                            catch (Exception ex)
                            {
                                this.Logger.LogError(ex, "Error writing frame {frameId} to output stream", currentFrame.FrameId);
                            }
                            finally
                            {
                                await writer.WriteAsync(currentFrame).ConfigureAwait(false);
                            }

                            if (currentFrame.FrameId % splitIntervalInFrames == 0)
                            {
                                this.Logger.LogInformation("Splitting output file {file} at frame {frameId}", currentFrame.FrameId);
                                goto outer;
                            }
                        }
                    }

#pragma warning disable SA1024 // stylecop has issues with labels
                outer:;
#pragma warning restore SA1024
                }
            }
        },
        "encode",
        writer: writer).ConfigureAwait(false);

    private async Task RunPipelineFragment(Func<Task> body, string name, Func<Task> final = default, ChannelWriter<OrderedFrame> writer = default)
    {
        this.Logger.LogDebug("Starting {pipelineFragment} loop", name);
        try
        {
            await body().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            this.investigationEvent.Reset();
            this.Logger.LogError(ex, "Error while running {pipelineFragment} loop", name);
        }
        finally
        {
            this.Logger.LogDebug("Stopping {pipelineFragment} loop", name);
            if (final is not null)
            {
                await final().ConfigureAwait(false);
            }

            writer?.Complete();
            this.Logger.LogError("{pipelineFragment} loop stopped", name);
        }
    }

    private record struct OrderedFrame(long FrameId, IPooledFrame Frame);
}
