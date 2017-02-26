using System;
using System.Drawing;
using SharpDX.DXGI;
using System.Drawing.Imaging;
using SharpDX.Direct3D11;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace kasthack.TimeLapser {
    internal partial class DXSnapper : DisposableBase, ISnapper {
        public const int renderPoolSize = 3;
        private const int destPixelSize = 3;
        private const int sourcePixelSize = sizeof(int);
        private const PixelFormat destPixelFormat = PixelFormat.Format24bppRgb;
        private const Format sourcePixelFormat = Format.B8G8R8A8_UNorm;

        private int currentRenderIndex = 0;
        private Factory1 _factory;
        private Bitmap[] _renderBitmaps;
        private Rectangle? _sourceRect;
        private DXSnapperInput[] _inputs;

        public int MaxProcessingThreads => renderPoolSize;

        public void SetSource(Rectangle sourceRect) {
            ThrowIfDisposed();
            DisposeNative();

            _sourceRect = sourceRect;
            _factory = new Factory1();
            _inputs = GetCapturedOutputs().Select(a => new DXSnapperInput(_factory, a.Item1, a.Item2, sourceRect)).ToArray();
            _renderBitmaps = Enumerable.Range(0, renderPoolSize).Select(a=>new Bitmap(sourceRect.Width, sourceRect.Height, destPixelFormat)).ToArray();
        }
        private Tuple<int, int>[] GetCapturedOutputs() {
            var ret = new List<Tuple<int,int>>(6);//most cases
            for (var adapterIndex = _factory.GetAdapterCount1() - 1; adapterIndex >= 0; adapterIndex--)
                using (var adapter = _factory.GetAdapter1(adapterIndex))
                    for (var outputIndex = adapter.GetOutputCount() - 1; outputIndex >= 0; outputIndex--)
                        using (var output = adapter.GetOutput(outputIndex))
                            if (output.Description.DesktopBounds.ToGDIRect().IntersectsWith(_sourceRect.Value))
                                ret.Add(new Tuple<int,int>(adapterIndex, outputIndex));
            return ret.ToArray();
        }
        public async Task<Bitmap> Snap(int timeout = 0) {
            ThrowIfDisposed();
            if (_sourceRect == null)
                throw new InvalidOperationException("You have to specify source");
            currentRenderIndex = ( currentRenderIndex + 1 ) % renderPoolSize;
            var renderBitmap = _renderBitmaps[currentRenderIndex];
            BitmapData bitmap = null;
            try {
                var boundsRect = new Rectangle(0, 0, renderBitmap.Width, renderBitmap.Height);
                bitmap = renderBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, renderBitmap.PixelFormat);

                await Task.WhenAll(_inputs.Select(input=>Task.Run(()=>input.Snap(bitmap, timeout)))).ConfigureAwait(false);
            }
            finally {
                // Release source and dest locks
                try {
                    renderBitmap.UnlockBits(bitmap);
                }
                catch (Exception ex) {
                    Debugger.Break();
                }
            }
            return renderBitmap;
        }
        public override void Dispose() {
            DisposeNative();
            base.Dispose();
        }

        private void DisposeNative() {
            _factory?.Dispose();
            _factory = null;

            if (_renderBitmaps != null) {
                for (var i = 0; i < _renderBitmaps.Length; i++) {
                    _renderBitmaps[i]?.Dispose();
                    _renderBitmaps[i] = null;
                }
                _renderBitmaps = null;
            }
            if (_inputs != null) {
                for (var i = 0; i < _inputs.Length; i++) { 
                    _inputs[i]?.Dispose();
                    _inputs[i] = null;
                }
                _inputs = null;
            }
        }
    }
}
