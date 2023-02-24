namespace kasthack.TimeLapser.Recording.Snappers.SDGSnapper
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using kasthack.TimeLapser.Recording.Snappers;

    using Microsoft.Extensions.Logging;

    /*
     * simple snapper, utilizing System.Drawing.Graphics.CopyFromScreen
     */
    internal class SDGSnapper : DisposableBase, ISnapper
    {
        private const int RenderPoolSize = 3;
        private readonly ILogger<SDGSnapper> logger;
        private int currentRenderIndex = 0;
        private Bitmap[] renderedFrames;
        private Graphics[] canvases;
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

                this.renderedFrames = Enumerable.Range(0, this.MaxProcessingThreads).Select(_ => new Bitmap(sourceRect.Width, sourceRect.Height)).ToArray();
                this.canvases = this.renderedFrames.Select(renderedFrame => Graphics.FromImage(renderedFrame)).ToArray();
                this.logger.LogDebug("Successfully set source rectangle {sourceRectangle} with render pool size {renderPoolSize}", sourceRect, this.MaxProcessingThreads);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to set source {sourceRectangle}", sourceRect);
                throw;
            }
        }

        public async Task<Bitmap> Snap(int timeout = 0)
        {
            await Task.CompletedTask;
            var stopwatch = Stopwatch.StartNew();
            this.logger.LogTrace("Capturing a screenshot with timeout {timeout}, source rectangle {sourceRectangle}", timeout);
            int currenRenderIndex = -1;
            try
            {
                _ = this.ThrowIfDisposed();
                if (this.sourceRectangle == null)
                {
                    throw new InvalidOperationException("You have to specify source");
                }

                currenRenderIndex = Interlocked.Increment(ref this.currentRenderIndex);
                var graphics = this.canvases[currenRenderIndex];
                var renderedFrame = this.renderedFrames[currenRenderIndex];

                var src = this.sourceRectangle.Value;
                graphics.CopyFromScreen(src.X, src.Y, 0, 0, renderedFrame.Size);
                graphics.Flush();
                this.logger.LogTrace("Captured a screenshot with timeout {timeout}, source rectangle {sourceRectangle}, render index {renderIndex}, took {elapsed}", timeout, this.sourceRectangle.Value, currenRenderIndex, stopwatch.Elapsed);
                return renderedFrame; // ok, that's a bad idea but we can't allocate a ton of memory for each frame
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

            if (this.canvases != null)
            {
                for (var i = 0; i < this.canvases.Length; i++)
                {
                    if (disposing)
                    {
                        this.canvases[i]?.Dispose();
                    }

                    this.canvases[i] = null;
                }

                this.canvases = null;
            }

            if (this.renderedFrames != null)
            {
                for (var i = 0; i < this.renderedFrames.Length; i++)
                {
                    if (disposing)
                    {
                        this.renderedFrames[i]?.Dispose();
                    }

                    this.renderedFrames[i] = null;
                }

                this.renderedFrames = null;
            }

            this.logger.LogDebug("Disposed snapper resources, disposing = {disposing}", disposing);
        }
    }
}
