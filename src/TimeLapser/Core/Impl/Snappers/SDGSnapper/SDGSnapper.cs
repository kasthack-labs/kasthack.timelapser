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
        private int currentRenderIndex = 0;
        private ObjectPool<RenderContext> renderPool;
        private Rectangle? sourceRectangle;

        public SDGSnapper(ILogger<SDGSnapper> logger)
        {
            this.logger = logger;
        }

        ~SDGSnapper()
        {
            this.Dispose(false);
        }

        public int MaxProcessingThreads => RenderPoolSize;

        public void SetSource(Rectangle sourceRect)
        {
            this.logger.LogTrace("Configuring source rectangle {sourceRectangle}", sourceRect);
            try
            {
                _ = this.ThrowIfDisposed();
                this.Dispose(true);
                this.sourceRectangle = sourceRect;

                this.renderPool = new DefaultObjectPoolProvider().Create(new FactoryObjectPoolPolicy<RenderContext>(() =>
                    {
                        var frame = new Bitmap(this.sourceRectangle.Value.Width, this.sourceRectangle.Value.Height);
                        var canvas = Graphics.FromImage(frame);
                        return new RenderContext(frame, canvas);
                    }));
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
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogTrace("Capturing a screenshot with timeout {timeout}, source rectangle {sourceRectangle}", timeout);
            var currenRenderIndex = -1;
            try
            {
                _ = this.ThrowIfDisposed();
                if (this.sourceRectangle == null)
                {
                    throw new InvalidOperationException("You have to specify source");
                }

                currenRenderIndex = Interlocked.Increment(ref this.currentRenderIndex);
                var renderContext = this.renderPool.GetDisposable();
                var (renderedFrame, graphics) = renderContext.Value;

                var src = this.sourceRectangle.Value;
                graphics.CopyFromScreen(src.X, src.Y, 0, 0, renderedFrame.Size);
                graphics.Flush();
                this.logger.LogTrace("Captured a screenshot with timeout {timeout}, source rectangle {sourceRectangle}, render index {renderIndex}, took {elapsed}", timeout, this.sourceRectangle.Value, currenRenderIndex, stopwatch.Elapsed);
                return new RenderContextPooledFrame(renderContext);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to capture a screenshot with timeout {timeout}, source rectangle {sourceRectangle}, render index {renderIndex}, took {elapsed}", timeout, this.sourceRectangle.Value, currenRenderIndex, stopwatch.Elapsed);
                throw;
            }
        }

        public override void Dispose()
        {
            this.logger.LogDebug("Disposing snapper with Dispose");
            this.Dispose(true);
            base.Dispose();
        }

        private void Dispose(bool disposing)
        {
            this.logger.LogDebug("Disposing snapper resources, disposing = {disposing}", disposing);
            this.sourceRectangle = null;

            this.renderPool = null;

            this.logger.LogDebug("Disposed snapper resources, disposing = {disposing}", disposing);
        }

        private record RenderContext(Bitmap frame, Graphics canvas);

        private record RenderContextPooledFrame(PooledWrapper<RenderContext> context) : IPooledFrame
        {
            public Bitmap Value => this.context.Value.frame;

            public void Dispose() => this.context.Dispose();
        }
    }
}
