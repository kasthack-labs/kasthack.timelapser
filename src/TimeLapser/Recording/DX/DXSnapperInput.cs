namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;

    internal partial class DXSnapper
    {
        private class DXSnapperInput : DisposableBase, IDisposable
        {
            private readonly int destXOffset;
            private readonly int destYOffset;
            private readonly int sourceXOffset;
            private readonly int sourceYOffset;
            private readonly int height;
            private readonly int width;

            private readonly Adapter1 adapter;
            private readonly Output1 output1;
            private readonly Output output;
            private readonly Texture2DDescription textureDescription;
            private readonly Texture2D screenTexture;
            private readonly SharpDX.Direct3D11.Device device;
            private readonly OutputDuplication duplicatedOutput;

            public DXSnapperInput(Factory1 factory, int adapterIndex, int outputIndex, Rectangle captureRectangle)
            {
                this.adapter = factory.GetAdapter1(adapterIndex);
                this.device = new SharpDX.Direct3D11.Device(this.adapter);
                this.output = this.adapter.GetOutput(outputIndex);
                var outputBounds = this.output.Description.DesktopBounds.ToGDIRect();

                var intersection = Rectangle.Intersect(outputBounds, captureRectangle);
                if (intersection.IsEmpty)
                {
                    this.Dispose();
                    throw new ArgumentOutOfRangeException(nameof(captureRectangle), $"Output {outputIndex} for adapter {adapterIndex} {FormatRectangle(outputBounds)} doesn't intersect with capture rectangle {FormatRectangle(captureRectangle)}");
                }

                this.textureDescription = new Texture2DDescription
                {
                    CpuAccessFlags = CpuAccessFlags.Read,
                    BindFlags = BindFlags.None,
                    Format = SourcePixelFormat,
                    Height = outputBounds.Height,
                    Width = outputBounds.Width,
                    OptionFlags = ResourceOptionFlags.None,
                    MipLevels = 1,
                    ArraySize = 1,
                    SampleDescription =
                    {
                        Count = 1,
                        Quality = 0,
                    },
                    Usage = ResourceUsage.Staging,
                };
                this.screenTexture = new Texture2D(this.device, this.textureDescription);
                this.output1 = this.output.QueryInterface<Output1>();
                this.duplicatedOutput = this.output1.DuplicateOutput(this.device);

                this.destXOffset = intersection.Left - captureRectangle.Left;
                this.destYOffset = intersection.Top - captureRectangle.Top;
                this.sourceXOffset = intersection.Left - outputBounds.Left;
                this.sourceYOffset = intersection.Top - outputBounds.Top;
                this.width = intersection.Width;
                this.height = intersection.Height;
            }

            /// <inheritdoc/>
            public override void Dispose()
            {
                this.adapter?.Dispose();
                this.device?.Dispose();
                this.output?.Dispose();
                this.output1?.Dispose();
                this.screenTexture?.Dispose();
                this.duplicatedOutput?.Dispose();
                base.Dispose();
            }

            internal bool Snap(BitmapData bitmap, int timeout)
            {
                _ = this.ThrowIfDisposed();
                SharpDX.DXGI.Resource screenResource = null;
                var acquiredFrame = false;
                try
                {
                    try
                    {
                        var result = this.duplicatedOutput.TryAcquireNextFrame(timeout, out var dfi, out screenResource);
                        if (!result.Success)
                        {
                            return false;
                        }

                        acquiredFrame = true;
                    }
                    catch (SharpDXException e) when (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                    {
                        return false;
                    }

                    using (var queryInterface = screenResource.QueryInterface<SharpDX.Direct3D11.Resource>())
                    {
                        this.device.ImmediateContext.CopyResource(queryInterface, this.screenTexture);
                    }

                    var databox = this.device.ImmediateContext.MapSubresource(this.screenTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                    this.Render(databox, bitmap);
                    return true;
                }
                finally
                {
                    this.device.ImmediateContext.UnmapSubresource(this.screenTexture, 0);
                    screenResource?.Dispose();
                    if (acquiredFrame)
                    {
                        this.duplicatedOutput?.ReleaseFrame();
                    }
                }
            }

            private static string FormatRectangle(Rectangle desktopBounds) => $"(x:{desktopBounds.X} y:{desktopBounds.Y} w:{desktopBounds.Width} h:{desktopBounds.Height})";

            private unsafe void Render(DataBox databox, BitmapData bitmap)
            {
                var sourcePtr = IntPtr.Add(databox.DataPointer, (this.sourceXOffset * SourcePixelSize) + (this.sourceYOffset * databox.RowPitch));
                var destPtr = IntPtr.Add(bitmap.Scan0, (this.destXOffset * DestPixelSize) + (this.destYOffset * bitmap.Stride));
                for (var y = 0; y < this.height; y++)
                {
                    /*
                    * Accord.Video.FFMPEG.VideoFileWriter.WriteVideoFrame internally uses bimtaps/Format24bppRgb and converts other formats
                    *   => it's faster to do this while copying the data
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
                    var destinationBytePointer = (byte*)destPtr.ToPointer();
                    var sourceInt32Pointer = (int*)sourcePtr.ToPointer();
                    var sourceWideEndPointer = sourceInt32Pointer + this.width - 1;
                    while (sourceInt32Pointer < sourceWideEndPointer)
                    {
                        *(int*)destinationBytePointer = *sourceInt32Pointer++;
                        destinationBytePointer += DestPixelSize;
                    }

                    if (this.width > 0)
                    {
                        var sourceBytePointer = (byte*)sourceInt32Pointer;
                        for (var i = 0; i < DestPixelSize; i++)
                        {
                            *destinationBytePointer++ = *sourceBytePointer++;
                        }
                    }

                    sourcePtr = IntPtr.Add(sourcePtr, databox.RowPitch);
                    destPtr = IntPtr.Add(destPtr, bitmap.Stride);
                }
            }
        }
    }
}
