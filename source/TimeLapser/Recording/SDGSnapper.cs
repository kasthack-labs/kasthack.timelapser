using System;
using System.Drawing;
using System.Threading.Tasks;

namespace kasthack.TimeLapser {
    /*
     * simple snapper, utilizing System.Drawing.Graphics.CopyFromScreen
     */
    class SDGSnapper : DisposableBase,ISnapper {
        private Bitmap _bmp;
        private Graphics _gr;
        private Rectangle? _sourceRectHolder;

        public int MaxProcessingThreads => 1;

        public void SetSource(Rectangle sourceRect) {
            ThrowIfDisposed();
            DisposeNative();
            _sourceRectHolder = sourceRect;

            _bmp = new Bitmap(sourceRect.Width, sourceRect.Height);
            _gr = Graphics.FromImage(_bmp);
        }
        public async Task<Bitmap> Snap(int interval = 0) {
            ThrowIfDisposed();
            if (_sourceRectHolder == null)
                throw new InvalidOperationException("You have to specify source");
            var src = _sourceRectHolder.Value;
            _gr.CopyFromScreen(src.X, src.Y, 0, 0, _bmp.Size);
            _gr.Flush();
            return _bmp;//ok, that's a bad idea but we can't allocate fuckton of memory for each frame
        }
        public override void Dispose() {
            DisposeNative();
            base.Dispose();
        }
        private void DisposeNative() {
            _gr?.Dispose();
            _bmp?.Dispose();
            _sourceRectHolder = null;
            _gr = null;
            _bmp = null;
        }
    }
}