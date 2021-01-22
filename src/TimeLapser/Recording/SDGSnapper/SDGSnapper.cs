using System;
using System.Drawing;
using System.Threading.Tasks;

namespace kasthack.TimeLapser
{
    /*
     * simple snapper, utilizing System.Drawing.Graphics.CopyFromScreen
     */
    class SDGSnapper : DisposableBase, ISnapper
    {
        private Bitmap renderedFrame;
        private Graphics graphics;
        private Rectangle? sourceRectangle;

        public int MaxProcessingThreads => 1;

        public void SetSource(Rectangle sourceRect)
        {
            ThrowIfDisposed();
            DisposeNative();
            sourceRectangle = sourceRect;

            renderedFrame = new Bitmap(sourceRect.Width, sourceRect.Height);
            graphics = Graphics.FromImage(renderedFrame);
        }
        public async Task<Bitmap> Snap(int interval = 0)
        {
            ThrowIfDisposed();
            if (sourceRectangle == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }
            var src = sourceRectangle.Value;
            graphics.CopyFromScreen(src.X, src.Y, 0, 0, renderedFrame.Size);
            graphics.Flush();
            return renderedFrame;//ok, that's a bad idea but we can't allocate fuckton of memory for each frame
        }
        public override void Dispose()
        {
            DisposeNative();
            base.Dispose();
        }
        private void DisposeNative()
        {
            graphics?.Dispose();
            renderedFrame?.Dispose();
            sourceRectangle = null;
            graphics = null;
            renderedFrame = null;
        }
    }
}