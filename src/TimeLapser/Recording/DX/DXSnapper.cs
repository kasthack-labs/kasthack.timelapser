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
    using SharpDX.DXGI;

    internal partial class DXSnapper : DisposableBase, ISnapper
    {
        public const int RenderPoolSize = 3;
        private const int DestPixelSize = 3;
        private const int SourcePixelSize = sizeof(int);
        private const PixelFormat DestPixelFormat = PixelFormat.Format24bppRgb;
        private const Format SourcePixelFormat = Format.B8G8R8A8_UNorm;

        private int currentRenderIndex = 0;
        private Factory1 factory;
        private Bitmap[] renderBitmaps;
        private Rectangle? sourceRect;
        private DXSnapperInput[] inputs;

        public int MaxProcessingThreads => RenderPoolSize;

        public void SetSource(Rectangle sourceRect)
        {
            _ = this.ThrowIfDisposed();
            this.DisposeNative();

            this.sourceRect = sourceRect;
            this.factory = new Factory1();
            this.inputs = this.GetCapturedOutputs().Select(a => new DXSnapperInput(this.factory, a.Item1, a.Item2, sourceRect)).ToArray();
            this.renderBitmaps = Enumerable.Range(0, RenderPoolSize).Select(_ => new Bitmap(sourceRect.Width, sourceRect.Height, DestPixelFormat)).ToArray();
        }

        public async Task<Bitmap> Snap(int timeout = 0)
        {
            this.ThrowIfDisposed();
            if (this.sourceRect == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }

            this.currentRenderIndex = (this.currentRenderIndex + 1) % RenderPoolSize;
            var renderBitmap = this.renderBitmaps[this.currentRenderIndex];
            BitmapData bitmap = null;
            try
            {
                var boundsRect = new Rectangle(0, 0, renderBitmap.Width, renderBitmap.Height);

                /*
                 * Parallel snap renders every input at the same time using shared pointer
                 * Single threaded snap renders input sequentally and recreates bitmapdata for each render
                 */

#if ParallelSnap
                bitmap = renderBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, renderBitmap.PixelFormat);
#endif

#if ParallelSnap
                await Task.WhenAll(
#endif
                    this.inputs.Select(input =>
#if ParallelSnap
                        Task.Run(() =>
                        {
#else
                            bitmap = renderBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, renderBitmap.PixelFormat);
#endif
                            input.Snap(bitmap, timeout);
#if !ParallelSnap
                            renderBitmap.UnlockBits(bitmap);
#else
                            return 0;
                        }
                    )
#endif
                )
#if ParallelSnap
                ).ConfigureAwait(false)
#else
                    .ToArray()
#endif
                ;
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
                catch (Exception)
                {
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
