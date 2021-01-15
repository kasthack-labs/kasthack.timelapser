#define ParallelSnap
using System;
using System.Drawing;
using SharpDX.DXGI;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;

namespace kasthack.TimeLapser
{
    internal partial class DXSnapper : DisposableBase, ISnapper
    {
        public const int renderPoolSize = 3;
        private const int destPixelSize = 3;
        private const int sourcePixelSize = sizeof(int);
        private const PixelFormat destPixelFormat = PixelFormat.Format24bppRgb;
        private const Format sourcePixelFormat = Format.B8G8R8A8_UNorm;

        private int currentRenderIndex = 0;
        private Factory1 factory;
        private Bitmap[] renderBitmaps;
        private Rectangle? sourceRect;
        private DXSnapperInput[] inputs;

        public int MaxProcessingThreads => renderPoolSize;

        public void SetSource(Rectangle sourceRect)
        {
            ThrowIfDisposed();
            DisposeNative();

            this.sourceRect = sourceRect;
            factory = new Factory1();
            inputs = GetCapturedOutputs().Select(a => new DXSnapperInput(factory, a.Item1, a.Item2, sourceRect)).ToArray();
            renderBitmaps = Enumerable.Range(0, renderPoolSize).Select(a => new Bitmap(sourceRect.Width, sourceRect.Height, destPixelFormat)).ToArray();
        }
        private Tuple<int, int>[] GetCapturedOutputs()
        {
            var ret = new List<Tuple<int, int>>(6);//most cases
            for (var adapterIndex = factory.GetAdapterCount1() - 1; adapterIndex >= 0; adapterIndex--)
            {
                using (var adapter = factory.GetAdapter1(adapterIndex))
                {
                    for (var outputIndex = adapter.GetOutputCount() - 1; outputIndex >= 0; outputIndex--)
                    {
                        using (var output = adapter.GetOutput(outputIndex))
                        {
                            if (output.Description.DesktopBounds.ToGDIRect().IntersectsWith(sourceRect.Value))
                            {
                                ret.Add(new Tuple<int, int>(adapterIndex, outputIndex));
                            }
                        }
                    }
                }
            }

            return ret.ToArray();
        }
        public async Task<Bitmap> Snap(int timeout = 0)
        {
            ThrowIfDisposed();
            if (sourceRect == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }

            currentRenderIndex = (currentRenderIndex + 1) % renderPoolSize;
            var renderBitmap = renderBitmaps[currentRenderIndex];
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
                    inputs.Select(input =>
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
                catch (Exception ex)
                {
                    Debugger.Break();
                }
            }
            return renderBitmap;
        }
        public override void Dispose()
        {
            DisposeNative();
            base.Dispose();
        }

        private void DisposeNative()
        {
            factory?.Dispose();
            factory = null;

            if (renderBitmaps != null)
            {
                for (var i = 0; i < renderBitmaps.Length; i++)
                {
                    renderBitmaps[i]?.Dispose();
                    renderBitmaps[i] = null;
                }
                renderBitmaps = null;
            }
            if (inputs != null)
            {
                for (var i = 0; i < inputs.Length; i++)
                {
                    inputs[i]?.Dispose();
                    inputs[i] = null;
                }
                inputs = null;
            }
        }
    }
}
