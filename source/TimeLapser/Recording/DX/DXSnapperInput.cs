using System;
using System.Drawing;
using SharpDX.DXGI;
using SharpDX;
using SharpDX.Direct3D11;
using System.Drawing.Imaging;

namespace kasthack.TimeLapser {
    internal partial class DXSnapper {
        private class DXSnapperInput : DisposableBase, IDisposable {
            private readonly int _destXOffset;
            private readonly int _destYOffset;
            private readonly int _sourceXOffset;
            private readonly int _sourceYOffset;
            private readonly int _height;
            private readonly int _width;

            private readonly Adapter1 _adapter;
            private readonly Output1 _output1;
            private readonly Output _output;
            private readonly Texture2DDescription _textureDescription;
            private readonly Texture2D _screenTexture;
            private readonly SharpDX.Direct3D11.Device _device;
            private readonly OutputDuplication _duplicatedOutput;

            public DXSnapperInput(Factory1 factory, int adapterIndex, int outputIndex, Rectangle captureRectangle) {
                _adapter = factory.GetAdapter1(adapterIndex);
                _device = new SharpDX.Direct3D11.Device(_adapter);
                _output = _adapter.GetOutput(outputIndex);
                var outputBounds = _output.Description.DesktopBounds.ToGDIRect();

                var intersection = Rectangle.Intersect(outputBounds, captureRectangle);
                if (intersection.IsEmpty) {
                    Dispose();
                    throw new ArgumentOutOfRangeException(nameof(captureRectangle), $"Output {outputIndex} for adapter {adapterIndex} {FormatRectangle(outputBounds)} doesn't intersect with capture rectangle {FormatRectangle(captureRectangle)}");
                }

                _textureDescription = new Texture2DDescription {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = sourcePixelFormat,
                    Height = outputBounds.Height,
                    Width = outputBounds.Width,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription = {
                    Count = 1,
                    Quality = 0
                },
                    Usage = ResourceUsage.Staging,
                };
                _screenTexture = new Texture2D(_device, _textureDescription);
                _output1 = _output.QueryInterface<Output1>();
                _duplicatedOutput = _output1.DuplicateOutput(_device);

                _destXOffset = intersection.Left - captureRectangle.Left;
                _destYOffset= intersection.Top - captureRectangle.Top;
                _sourceXOffset = intersection.Left - outputBounds.Left;
                _sourceYOffset = intersection.Top - outputBounds.Top;
                _width = intersection.Width;
                _height = intersection.Height;
            }
            internal bool Snap(BitmapData bitmap, int timeout) {
                ThrowIfDisposed();
                SharpDX.DXGI.Resource screenResource = null;
                var acquiredFrame = false;
                try {
                    try {
                        OutputDuplicateFrameInformation dfi;
                        _duplicatedOutput.AcquireNextFrame(timeout, out dfi, out screenResource);
                        acquiredFrame = true;
                    }
                    catch (SharpDXException e) when (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code) {
                        return false;
                    }
                    using (var queryInterface = screenResource.QueryInterface<SharpDX.Direct3D11.Resource>())
                        _device.ImmediateContext.CopyResource(queryInterface, _screenTexture);
                    var databox = _device.ImmediateContext.MapSubresource(_screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                    Render(databox, bitmap);
                    return true;
                }
                finally {
                    _device.ImmediateContext.UnmapSubresource(_screenTexture, 0);
                    screenResource?.Dispose();
                    if (acquiredFrame)
                        _duplicatedOutput?.ReleaseFrame();
                }
            }
            private unsafe void Render(DataBox databox, BitmapData bitmap) {
                var sourcePtr = IntPtr.Add(databox.DataPointer, _sourceXOffset * sourcePixelSize + _sourceYOffset * databox.RowPitch);
                var destPtr = IntPtr.Add(bitmap.Scan0, _destXOffset * destPixelSize + _destYOffset * bitmap.Stride);
                for (var y = 0; y < _height; y++) {
                    /*
                    * Accord.Video.FFMPEG.VideoFileWriter.WriteVideoFrame internally uses bimtaps/Format24bppRgb and converts other formats => it's faster to do this while copying the data
                    * https://github.com/accord-net/framework/blob/development/Sources/Extras/Accord.Video.FFMPEG.GPL/VideoFileWriter.cpp#L600-L621
                    *
                    * 
                    * fast B8G8R8A8_UNorm -> Format24bppRgb
                    * - 24bppRGB is actually BGR
                    * - alpha is always 0 => we can just copy raw data
                    *
                    * destPixelSize = 3:
                    * 
                    * for body:
                    * 
                    * |B|G|R|A|
                    *       |B|G|R|A|
                    *
                    *             |B|G|R|         last pixel in a row / if (width > 0)
                    */
                    var bptr = (byte*)destPtr.ToPointer();
                    var sptr = (int*)sourcePtr.ToPointer();
                    var sw = sptr + _width - 1;
                    while(sptr < sw) {
                        *(int*)bptr = *sptr++;
                        bptr += destPixelSize;
                    }
                    if (_width > 0) {
                        var sbptr = (byte*)sptr;
                        *bptr++ = *sbptr++;
                        *bptr++ = *sbptr++;
                        *bptr++ = *sbptr++;
                    }
                    sourcePtr = IntPtr.Add(sourcePtr, databox.RowPitch);
                    destPtr = IntPtr.Add(destPtr, bitmap.Stride);
                }
            }
            public override void Dispose() {
                _adapter?.Dispose();
                _device?.Dispose();
                _output?.Dispose();
                _output1?.Dispose();
                _screenTexture?.Dispose();
                _duplicatedOutput?.Dispose();
                base.Dispose();
            }
            private static string FormatRectangle(Rectangle desktopBounds) => $"(x:{desktopBounds.X} y:{desktopBounds.Y} w:{desktopBounds.Width} h:{desktopBounds.Height})";
        }
    }
}