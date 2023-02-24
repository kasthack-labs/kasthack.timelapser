#define ParallelSnap
namespace kasthack.TimeLapser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Linq;
    using System.Threading.Tasks;

    using kasthack.TimeLapser.Recording.Snappers;
    using kasthack.TimeLapser.Recording.Snappers.DX;

    using Microsoft.Extensions.Logging;

    using SharpDX.DXGI;

    internal partial class DXSnapper : DisposableBase, ISnapper
    {
        public const int RenderPoolSize = 3;
        private const int DestPixelSize = 3;
        private const int SourcePixelSize = sizeof(int);
        private const PixelFormat DestPixelFormat = PixelFormat.Format24bppRgb;
        private const Format SourcePixelFormat = Format.B8G8R8A8_UNorm;
        private readonly ILogger<DXSnapper> logger;
        private int currentRenderIndex = 0;
        private Factory1 factory;
        private Bitmap[] renderBitmaps;
        private Rectangle? sourceRect;
        private DXSnapperInput[] inputs;

        public DXSnapper(ILogger<DXSnapper> logger)
        {
            this.logger = logger;
        }

        public int MaxProcessingThreads => RenderPoolSize;

        public void SetSource(Rectangle sourceRectangle)
        {
            this.logger.LogDebug("Setting source rectangle to {sourceRectangle}", sourceRectangle);
            _ = this.ThrowIfDisposed();
            this.DisposeNative();

            this.sourceRect = sourceRectangle;
            this.factory = new Factory1();
            this.inputs = this.GetCapturedOutputs().Select(a => new DXSnapperInput(this.factory, a.Item1, a.Item2, sourceRectangle, this.logger)).ToArray();
            this.renderBitmaps = Enumerable.Range(0, RenderPoolSize).Select(_ => new Bitmap(sourceRectangle.Width, sourceRectangle.Height, DestPixelFormat)).ToArray();
            this.logger.LogTrace("Set source rectangle to {sourceRectangle}", sourceRectangle);
        }

        public async Task<Bitmap> Snap(int timeout = 0)
        {
            _ = this.ThrowIfDisposed();
            if (this.sourceRect == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }

            var renderIndex = this.currentRenderIndex = (this.currentRenderIndex + 1) % RenderPoolSize;
            var renderBitmap = this.renderBitmaps[renderIndex];
            this.logger.LogTrace("Capturing input using render index {renderIndex}", renderIndex);
            BitmapData bitmap = null;
            try
            {
                var boundsRect = new Rectangle(0, 0, renderBitmap.Width, renderBitmap.Height);

                /*
                 * Parallel snap renders every input at the same time using shared pointer
                 * Single threaded snap renders input sequentally and recreates bitmapdata for each render
                 */
                this.logger.LogTrace("Launching capture tasks for render index {renderIndex}", renderIndex);
                bitmap = renderBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, renderBitmap.PixelFormat);
                await Task.WhenAll(
                    this.inputs.Select(input =>
                        Task.Run(() => input.Snap(bitmap, timeout)))).ConfigureAwait(false);
                this.logger.LogTrace("Completed capture tasks for render index {renderIndex}", renderIndex);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Failed to capture input");
                throw;
            }
            finally
            {
                // Release source and dest locks
                try
                {
#if ParallelSnap
                    renderBitmap.UnlockBits(bitmap);
#endif
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "Failed to unlock rendering bitmap {renderIndex}", renderIndex);
                    Debugger.Break();
                }
            }

            return renderBitmap;
        }

        public override void Dispose()
        {
            this.DisposeNative();
            base.Dispose();
        }

        private Tuple<int, int>[] GetCapturedOutputs()
        {
            this.logger.LogTrace("Getting captured outputs");

            var ret = new List<Tuple<int, int>>(6); // most cases
            for (var adapterIndex = this.factory.GetAdapterCount1() - 1; adapterIndex >= 0; adapterIndex--)
            {
                using var adapter = this.factory.GetAdapter1(adapterIndex);
                for (var outputIndex = adapter.GetOutputCount() - 1; outputIndex >= 0; outputIndex--)
                {
                    using var output = adapter.GetOutput(outputIndex);
                    if (output.Description.DesktopBounds.ToGDIRect().IntersectsWith(this.sourceRect.Value))
                    {
                        ret.Add(new Tuple<int, int>(adapterIndex, outputIndex));
                    }
                }
            }

            return ret.ToArray();
        }

        private void DisposeNative()
        {
            this.factory?.Dispose();
            this.factory = null;

            if (this.renderBitmaps != null)
            {
                for (var i = 0; i < this.renderBitmaps.Length; i++)
                {
                    this.renderBitmaps[i]?.Dispose();
                    this.renderBitmaps[i] = null;
                }

                this.renderBitmaps = null;
            }

            if (this.inputs != null)
            {
                for (var i = 0; i < this.inputs.Length; i++)
                {
                    this.inputs[i]?.Dispose();
                    this.inputs[i] = null;
                }

                this.inputs = null;
            }
        }
    }
}
