namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;
    using System.Threading.Tasks;

    /*
     * simple snapper, utilizing System.Drawing.Graphics.CopyFromScreen
     */
    internal class SDGSnapper : DisposableBase, ISnapper
    {
        private Bitmap renderedFrame;
        private Graphics graphics;
        private Rectangle? sourceRectangle;

        public int MaxProcessingThreads => 1;

        public void SetSource(Rectangle sourceRect)
        {
            _ = this.ThrowIfDisposed();
            this.DisposeNative();
            this.sourceRectangle = sourceRect;

            this.renderedFrame = new Bitmap(sourceRect.Width, sourceRect.Height);
            this.graphics = Graphics.FromImage(this.renderedFrame);
        }

        public Task<Bitmap> Snap(int interval = 0)
        {
            _ = this.ThrowIfDisposed();
            if (this.sourceRectangle == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }

            var src = this.sourceRectangle.Value;
            this.graphics.CopyFromScreen(src.X, src.Y, 0, 0, this.renderedFrame.Size);
            this.graphics.Flush();
            return Task.FromResult(this.renderedFrame); // ok, that's a bad idea but we can't allocate fuckton of memory for each frame
        }

        public override void Dispose()
        {
            this.DisposeNative();
            base.Dispose();
        }

        private void DisposeNative()
        {
            this.graphics?.Dispose();
            this.renderedFrame?.Dispose();
            this.sourceRectangle = null;
            this.graphics = null;
            this.renderedFrame = null;
        }
    }
}
