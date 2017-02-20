using System;
using System.Drawing;
using SharpDX.DXGI;
using System.Diagnostics;
using SharpDX.Mathematics.Interop;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct3D11;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace TimeLapser {
    class DXSnapper : DisposableBase, ISnapper {
        const int pixelSize = sizeof(int);
        private const PixelFormat pixelFormat = PixelFormat.Format24bppRgb;//PixelFormat.Format32bppArgb;
        private const Format texturePixelFormat = Format.B8G8R8A8_UNorm;
        private OutputDuplication _duplicatedOutput;
        private SharpDX.Direct3D11.Device _device;
        private Texture2D _screenTexture;
        private Bitmap _renderBitmap;
        private Output1 _output1;
        private Factory1 _factory;
        private Adapter1 _adapter;
        private Output _output;

        private Texture2DDescription _texdes;
        private Rectangle? _sourceRect;
        private byte[] _rgbValues;

        public void SetSource(Rectangle sourceRect) {
            ThrowIfDisposed();
            DisposeNative();
            _sourceRect = sourceRect;
            var numAdapter = 0; // # of graphics card adapter
            var numOutput = 0; // # of output device (i.e. monitor)
            // create device and factory
            _factory = new Factory1();
            _adapter = _factory.GetAdapter1(numAdapter);
            _output = _adapter.GetOutput(numOutput);
            _device = new SharpDX.Direct3D11.Device(_adapter);
            // creating CPU-accessible texture resource
            var desktopBounds = _output.Description.DesktopBounds;
            _texdes = new Texture2DDescription {
                CpuAccessFlags = CpuAccessFlags.Read,
                BindFlags = BindFlags.None,
                Format = texturePixelFormat,
                Height = desktopBounds.Height(),
                Width = desktopBounds.Width(),
                OptionFlags = ResourceOptionFlags.None,
                MipLevels = 1,
                ArraySize = 1,
                SampleDescription = {
                    Count = 1,
                    Quality = 0
                },
                Usage = ResourceUsage.Staging
            };
            _screenTexture = new Texture2D(_device, _texdes);
            // duplicate output stuff
            _output1 = _output.QueryInterface<Output1>();
            _duplicatedOutput = _output1.DuplicateOutput(_device);
            _renderBitmap = new Bitmap(sourceRect.Width, sourceRect.Height, pixelFormat);
            _rgbValues = new byte[( sourceRect.Width * sourceRect.Height * pixelSize )];
        }
        public Bitmap Snap(int timeout=0) {
            ThrowIfDisposed();
            if (_sourceRect == null)
                throw new InvalidOperationException("You have to specify source");
            SharpDX.DXGI.Resource screenResource = null;
            //Surface screenSurface = null;
            //SharpDX.DataStream dataStream = null;
            DataBox mapSource;
            var acquiredFrame = false;
            try {
                try {
                    _duplicatedOutput.AcquireNextFrame(timeout, out var dfi, out screenResource);
                    acquiredFrame = true;
                } catch (SharpDX.SharpDXException e) when (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code) {
                    return null;
                }
                using (var queryInterface = screenResource.QueryInterface<SharpDX.Direct3D11.Resource>())
                    _device.ImmediateContext.CopyResource(queryInterface, _screenTexture);
                mapSource = _device.ImmediateContext.MapSubresource(_screenTexture, 0, MapMode.Read, MapFlags.None);
                return getImageFromDatabox(mapSource);
            } finally {
                _device.ImmediateContext.UnmapSubresource(_screenTexture, 0);

                screenResource?.Dispose();
                if (acquiredFrame)
                    _duplicatedOutput?.ReleaseFrame();
            }
        }
        unsafe Bitmap getImageFromDatabox(DataBox mapSource) {
            var width = _sourceRect.Value.Width;
            var height = _sourceRect.Value.Height;
            var boundsRect = new Rectangle(0, 0, width, height);
            var mapDest = _renderBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, _renderBitmap.PixelFormat);
            var sourcePtr = mapSource.DataPointer;
            var destPtr = mapDest.Scan0;
            for (var y = 0; y < height; y++) {
                //Utilities.CopyMemory(destPtr, sourcePtr, width * pixelSize);
                {//fast B8G8R8A8_UNorm -> Format24bppRgb
                    var bptr = (byte*)destPtr.ToPointer();
                    var sptr = (int*)sourcePtr.ToPointer();
                    var sw = width - 1;
                    for (var i = 0; i < sw; i++) {
                        *(int*)bptr = *sptr;
                        bptr += 3;
                        sptr++;
                    }
                    if (width > 0) {
                        var sbptr = (byte*)sptr;
                        *bptr++ = *sbptr++;
                        *bptr++ = *sbptr++;
                        *bptr++ = *sbptr++;
                    }
                }

                // Advance pointers
                sourcePtr = IntPtr.Add(sourcePtr, mapSource.RowPitch);
                destPtr = IntPtr.Add(destPtr, mapDest.Stride);
            }

            // Release source and dest locks
            _renderBitmap.UnlockBits(mapDest);
            return _renderBitmap;
        }
        //}
        public override void Dispose() {
            DisposeNative();
            base.Dispose();
        }

        private void DisposeNative() {
            _duplicatedOutput?.Dispose();
            _device?.Dispose();
            _screenTexture?.Dispose();
            _renderBitmap?.Dispose();
            _output1?.Dispose();
            _factory?.Dispose();
            _adapter?.Dispose();
            _output?.Dispose();

            _output = null;
            _adapter = null;
            _factory = null;
            _rgbValues = null;
            _output1 = null;
            _renderBitmap = null;
            _screenTexture = null;
            _device = null;
            _duplicatedOutput = null;
        }
    }
    public static class DXExtensions {
        public static int Height(this RawRectangle rect) => rect.Bottom - rect.Top;
        public static int Width(this RawRectangle rect) => rect.Right - rect.Left;
    }
}
