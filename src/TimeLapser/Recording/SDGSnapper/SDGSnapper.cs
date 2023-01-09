namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /*
     * simple snapper, utilizing System.Drawing.Graphics.CopyFromScreen
     */
    internal class SDGSnapper : DisposableBase, ISnapper
    {
        private const int RenderPoolSize = 3;
        private int currentRenderIndex = 0;
        private Bitmap[] renderedFrames;
        private Graphics[] canvases;
        private Rectangle? sourceRectangle;

        public int MaxProcessingThreads => RenderPoolSize;

        public void SetSource(Rectangle sourceRect)
        {
            _ = this.ThrowIfDisposed();
            this.DisposeNative();
            this.sourceRectangle = sourceRect;

            this.renderedFrames = Enumerable.Range(0, RenderPoolSize).Select(_ => new Bitmap(sourceRect.Width, sourceRect.Height)).ToArray();
            this.canvases = this.renderedFrames.Select(renderedFrame => Graphics.FromImage(renderedFrame)).ToArray();
        }

        public Task<Bitmap> Snap(int interval = 0)
        {
            _ = this.ThrowIfDisposed();
            if (this.sourceRectangle == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }

            var currenRenderIndex = Interlocked.Increment(ref this.currentRenderIndex);
            var graphics = this.canvases[currenRenderIndex];
            var renderedFrame = this.renderedFrames[currenRenderIndex];

            var src = this.sourceRectangle.Value;
            graphics.CopyFromScreen(src.X, src.Y, 0, 0, renderedFrame.Size);
            graphics.Flush();
            return Task.FromResult(renderedFrame); // ok, that's a bad idea but we can't allocate a ton of memory for each frame
        }

        public override void Dispose()
        {
            this.DisposeNative();
            base.Dispose();
        }

        private void DisposeNative()
        {
            this.sourceRectangle = null;

            if (this.canvases != null)
            {
                for (var i = 0; i < this.canvases.Length; i++)
                {
                    this.canvases[i]?.Dispose();
                    this.canvases[i] = null;
                }

                this.canvases = null;
            }

            if (this.renderedFrames != null)
            {
                for (var i = 0; i < this.renderedFrames.Length; i++)
                {
                    this.renderedFrames[i]?.Dispose();
                    this.renderedFrames[i] = null;
                }

                this.renderedFrames = null;
            }
        }
    }
}
