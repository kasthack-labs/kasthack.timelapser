namespace kasthack.TimeLapser.Core.Impl.Snappers.SDGSnapper
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Threading;
    using System.Threading.Tasks;

    using kasthack.TimeLapser.Core.Impl.Pooling;
    using kasthack.TimeLapser.Core.Impl.Util;
    using kasthack.TimeLapser.Core.Interfaces;
    using kasthack.TimeLapser.Core.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// Simple snappper that uses GDI+ to capture screenshots.
    /// </summary>
    internal class SDGSnapper : DisposableBase, ISnapper
    {
        private const int RenderPoolSize = 6;
        private readonly ILogger<SDGSnapper> logger;

        private ObjectPool<RenderContext> renderPool;

        private Rectangle? sourceRectangle;

        public SDGSnapper(ILogger<SDGSnapper> logger) => this.logger = logger;

        ~SDGSnapper() => this.DisposeNative(false);

        public int MaxProcessingThreads => RenderPoolSize;

        public void SetSource(Rectangle sourceRect)
        {
            this.logger.LogTrace("Configuring source rectangle {sourceRectangle}", sourceRect);
            try
            {
                _ = this.ThrowIfDisposed();
                this.DisposeNative(true);
                this.sourceRectangle = sourceRect;

                this.renderPool = ObjectPoolFactory.Create(
                    () =>
                    {
                        var sourceRect = this.sourceRectangle.Value;
                        this.logger.LogTrace("Creating new source context, source rect: {sourceRectangle}", sourceRect);
                        try
                        {
                            var frame = new Bitmap(sourceRect.Width, sourceRect.Height);
                            var canvas = Graphics.FromImage(frame);
                            return new RenderContext(frame, canvas, this.logger);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, "Failed to create new source context, source rect: {sourceRectangle}", sourceRect);
                            throw;
                        }
                    },
                    RenderPoolSize);
                this.logger.LogDebug("Successfully set source rectangle {sourceRectangle} with render pool size {renderPoolSize}", sourceRect, this.MaxProcessingThreads);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to set source {sourceRectangle}", sourceRect);
                throw;
            }
        }

        public async Task<IPooledFrame> Snap(int timeout = 0)
        {
            await Task.CompletedTask.ConfigureAwait(false);
            this.logger.LogTrace("Capturing a screenshot with timeout {timeout}, source rectangle {sourceRectangle}", timeout, this.sourceRectangle);
            try
            {
                _ = this.ThrowIfDisposed();
                if (this.sourceRectangle == null)
                {
                    throw new InvalidOperationException("You have to specify source");
                }

                var renderContext = this.renderPool.GetDisposable(this.logger);
                var (renderedFrame, graphics, _) = renderContext.Value;

                var rectangle = this.sourceRectangle.Value;
                graphics.CopyFromScreen(rectangle.X, rectangle.Y, 0, 0, renderedFrame.Size);
                graphics.Flush();

                this.logger.LogTrace("Captured a screenshot with timeout {timeout}, source rectangle {sourceRectangle}", timeout, this.sourceRectangle.Value);

                return new RenderContextPooledFrame(renderContext, this.logger);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to capture a screenshot with timeout {timeout}, source rectangle {sourceRectangle}", timeout, this.sourceRectangle.Value);
                throw;
            }
        }

        public override void Dispose()
        {
            this.logger.LogDebug("Disposing snapper with Dispose");
            this.DisposeNative(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }

        private void DisposeNative(bool disposing)
        {
            this.logger.LogDebug("Disposing snapper resources, disposing = {disposing}", disposing);
            this.sourceRectangle = null;

            // https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool?view=aspnetcore-7.0#:~:text=When%20DefaultObjectPoolProvider%20is%20used%20and
            // render pool is SOMETIMES IDisposable
            if (disposing && this.renderPool is IDisposable disposablePool)
            {
                this.logger.LogTrace("Render pool is disposable, disposing it");
                disposablePool.Dispose();
            }

            this.renderPool = null;

            this.logger.LogDebug("Disposed snapper resources, disposing = {disposing}", disposing);
        }

        private record RenderContext(Bitmap Frame, Graphics Canvas, ILogger Logger) : IDisposable
        {
            private static long idCounter = 0;

            private readonly long id = Interlocked.Increment(ref idCounter);

            public void Dispose()
            {
                this.Logger.LogTrace($"Disposing {nameof(RenderContext)}, id = {{id}}", this.id);
                this.Canvas.Dispose();
                this.Frame.Dispose();
                GC.SuppressFinalize(this);
            }
        }

        private record RenderContextPooledFrame(PooledWrapper<RenderContext> RenderContext, ILogger Logger) : IPooledFrame
        {
            public Bitmap Value => this.RenderContext.Value.Frame;

            public void Dispose()
            {
                this.Logger.LogTrace($"Releasing {nameof(RenderContextPooledFrame)}");
                this.RenderContext.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
