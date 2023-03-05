namespace kasthack.TimeLapser
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;

    using kasthack.TimeLapser.Core.Impl.Snappers.DX;
    using kasthack.TimeLapser.Core.Impl.Util;

    using Microsoft.Extensions.Logging;

    using SharpDX;
    using SharpDX.Direct3D11;
    using SharpDX.DXGI;

    /// <summary>
    /// Screen-scoped directX snapper.
    /// </summary>
    internal partial class DXSnapper
    {
        private class DXSnapperInput : DisposableBase
        {
            private const Format SourcePixelFormat = Format.B8G8R8A8_UNorm;

            private const int DestPixelSize = 3;
            private const int SourcePixelSize = sizeof(int);

            private readonly ILogger logger;

            private readonly int destXOffset;
            private readonly int destYOffset;
            private readonly int sourceXOffset;
            private readonly int sourceYOffset;
            private readonly int height;
            private readonly int width;

            private Factory1 factory;
            private Adapter1 adapter;
            private Output1 output1;
            private Output output;
            private Texture2DDescription textureDescription;
            private SharpDX.Direct3D11.Device device;
            private OutputDuplication duplicatedOutput;

            public DXSnapperInput(int adapterIndex, int outputIndex, Rectangle captureRectangle, ILogger logger)
            {
                this.logger = logger;
                this.logger.LogDebug("Creating DX input for {adapter} using capture rectangle {sourceRectangle}", adapterIndex, captureRectangle);
                this.factory = new Factory1();
                this.adapter = this.factory.GetAdapter1(adapterIndex);
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

                // this.screenTexture = new Texture2D(this.device, this.textureDescription);
                this.output1 = this.output.QueryInterface<Output1>();
                this.duplicatedOutput = this.output1.DuplicateOutput(this.device);

                this.destXOffset = intersection.Left - captureRectangle.Left;
                this.destYOffset = intersection.Top - captureRectangle.Top;
                this.sourceXOffset = intersection.Left - outputBounds.Left;
                this.sourceYOffset = intersection.Top - outputBounds.Top;
                this.width = intersection.Width;
                this.height = intersection.Height;
                this.logger.LogDebug("Created DX input for {adapter} using capture rectangle {sourceRectangle}", adapterIndex, captureRectangle);
            }

            private string AdapterDescription => $"{this.output.Description.DeviceName}({this.width}x{this.height} @ {this.sourceXOffset}x{this.sourceYOffset})";

            /// <inheritdoc/>
            public override void Dispose()
            {
                this.logger.LogDebug("Disposing DX input");
                this.factory.Dispose();
                this.adapter?.Dispose();
                this.device?.Dispose();
                this.output?.Dispose();
                this.output1?.Dispose();
                this.duplicatedOutput?.Dispose();

                this.factory = null;
                this.adapter = null;
                this.device = null;
                this.output = null;
                this.output1 = null;
                this.duplicatedOutput = null;

                base.Dispose();
                GC.SuppressFinalize(this);
            }

            internal bool Snap(BitmapData bitmap, int timeout)
            {
                _ = this.ThrowIfDisposed();
                if (bitmap.PixelFormat != SupportedPixelFormat)
                {
                    this.logger.LogError("Rendering is only supported for {supportedPixelFormat} bitmaps, got {outputPixelFormat} instead", SupportedPixelFormat, bitmap.PixelFormat);
                    throw new ArgumentOutOfRangeException(nameof(bitmap), $"Invalid pixel format for rendering: {SupportedPixelFormat} supported, got {bitmap.PixelFormat}");
                }

                this.logger.LogTrace("Snapping data into a bitmap using timeout {timeout} for input {input}", timeout, this.AdapterDescription);
                try
                {
                    using var cpuAccessibleTexture = this.GetDesktopTexture(timeout);
                    if (cpuAccessibleTexture is null)
                    {
                        return false;
                    }

                    try
                    {
                        this.logger.LogTrace("Mapping databox using input {input}", this.AdapterDescription);
                        var databox = this.device.ImmediateContext.MapSubresource(cpuAccessibleTexture, 0, MapMode.Read, SharpDX.Direct3D11.MapFlags.None);
                        this.logger.LogTrace("Mapped databox using input {input}, now rendering", this.AdapterDescription);
                        this.Render(databox, bitmap);
                        this.logger.LogTrace("Rendered using {input}", this.AdapterDescription);
                        return true;
                    }
                    finally
                    {
                        this.logger.LogTrace("Unmapping screen texture using {input}", this.AdapterDescription);
                        this.device.ImmediateContext.UnmapSubresource(cpuAccessibleTexture, 0);
                        this.logger.LogTrace("Unmapped screen texture using {input}", this.AdapterDescription);
                    }
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to capture frame for input {input}", this.AdapterDescription);
                    return false;
                }
            }

            private static string FormatRectangle(Rectangle desktopBounds) => $"(x:{desktopBounds.X} y:{desktopBounds.Y} w:{desktopBounds.Width} h:{desktopBounds.Height})";

            private Texture2D GetDesktopTexture(int timeout)
            {
                this.logger.LogTrace("Acquiring duplicated output frame for input {input}", this.AdapterDescription);
                SharpDX.DXGI.Resource desktopScreenResource;
                try
                {
                    var duplicatedOutputCaptureResult = this.duplicatedOutput.TryAcquireNextFrame(timeout, out var desktopFrameInformation, out desktopScreenResource);
                    if (!duplicatedOutputCaptureResult.Success)
                    {
                        this.logger.LogWarning("Failed to acquire duplicated output frame for input {input}", this.AdapterDescription);
                        return null;
                    }
                }
                catch (SharpDXException e) when (e.ResultCode.Code == SharpDX.DXGI.ResultCode.WaitTimeout.Result.Code)
                {
                    this.logger.LogWarning(e, "Failed to acquire duplicated output frame for input {input}", this.AdapterDescription);
                    return null;
                }

                this.logger.LogTrace("Acquired duplicated output frame for input {input}", this.AdapterDescription);

                try
                {
                    using (desktopScreenResource)
                    {
                        var cpuAccessibleTexture = new Texture2D(this.device, this.textureDescription);
                        using (var queryInterface = desktopScreenResource.QueryInterface<SharpDX.Direct3D11.Resource>())
                        {
                            this.logger.LogTrace("Copying duplicated output into texture using input {input}", this.AdapterDescription);
                            this.device.ImmediateContext.CopyResource(queryInterface, cpuAccessibleTexture);
                            this.logger.LogTrace("Copied duplicated output into texture using input {input}", this.AdapterDescription);
                        }

                        return cpuAccessibleTexture;
                    }
                }
                finally
                {
                    try
                    {
                        this.duplicatedOutput?.ReleaseFrame();
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError(ex, "Failed to release frame for input {input}", this.AdapterDescription);
                    }
                }
            }

            private unsafe void Render(DataBox databox, BitmapData bitmap)
            {
                this.logger.LogTrace("Rendering databox into a bitmap for {input}", this.AdapterDescription);
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

                this.logger.LogTrace("Rendered databox into a bitmap for {input}", this.AdapterDescription);
            }
        }
    }
}
