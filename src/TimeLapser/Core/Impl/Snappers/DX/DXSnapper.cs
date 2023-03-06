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

    using kasthack.TimeLapser.Core.Impl.Pooling;
    using kasthack.TimeLapser.Core.Impl.Snappers.DX;
    using kasthack.TimeLapser.Core.Impl.Util;
    using kasthack.TimeLapser.Core.Interfaces;
    using kasthack.TimeLapser.Core.Models;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;


    /// <summary>
    /// Snapper, utilizing DirectX.
    /// </summary>
    internal partial class DXSnapper : DisposableBase, ISnapper
    {
        public const int RenderPoolSize = 6;
        private const PixelFormat SupportedPixelFormat = PixelFormat.Format24bppRgb;

        private readonly ILogger<DXSnapper> logger;

        private System.Drawing.Rectangle? sourceRect;

        private ObjectPool<Bitmap> renderPool;
        private DXSnapperInput[] inputs;

        public DXSnapper(ILogger<DXSnapper> logger) => this.logger = logger;

        ~DXSnapper()
        {
            this.DisposeNative(false);
        }

        public int MaxProcessingThreads => RenderPoolSize;

        public void SetSource(Rectangle sourceRectangle)
        {
            this.logger.LogDebug("Setting source rectangle to {sourceRectangle}", sourceRectangle);
            _ = this.ThrowIfDisposed();
            this.DisposeNative(true);

            this.sourceRect = sourceRectangle;
            this.inputs = this.GetCapturedOutputs().Select(a => new DXSnapperInput(a.AdapterIndex, a.OutputIndex, sourceRectangle, this.logger)).ToArray();
            this.renderPool = ObjectPoolFactory.Create(
                    () =>
                    {
                        var rect = this.sourceRect.Value;
                        try
                        {
                            return new Bitmap(rect.Width, rect.Height, SupportedPixelFormat);
                        }
                        catch (Exception ex)
                        {
                            this.logger.LogError(ex, "Failed to create new source context, source rect: {sourceRectangle}", rect);
                            throw;
                        }
                    },
                    RenderPoolSize);
            this.logger.LogTrace("Set source rectangle to {sourceRectangle}", sourceRectangle);
        }

        public async Task<IPooledFrame> Snap(int timeout = 0)
        {
            this.logger.LogTrace("Capturing input");
            _ = this.ThrowIfDisposed();
            if (this.sourceRect == null)
            {
                throw new InvalidOperationException("You have to specify source");
            }

            var renderBitmapDisposable = this.renderPool.GetDisposable(this.logger);
            var renderBitmap = renderBitmapDisposable.Value;

            BitmapData bitmap = null;
            try
            {
                var boundsRect = new System.Drawing.Rectangle(0, 0, renderBitmap.Width, renderBitmap.Height);

                /*
                 * Parallel snap renders every input at the same time using shared pointer
                 * Single threaded snap renders input sequentally and recreates bitmapdata for each render
                 */
                this.logger.LogTrace("Launching capture tasks");
                bitmap = renderBitmap.LockBits(boundsRect, ImageLockMode.WriteOnly, renderBitmap.PixelFormat);
                await Task.WhenAll(
                    this.inputs.Select(input =>
                        Task.Run(() => input.Snap(bitmap, timeout)))).ConfigureAwait(false);
                this.logger.LogTrace("Completed capture tasks");
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
                    this.logger.LogError(ex, "Failed to unlock rendering bitmap");
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }
                }
            }

            return new PooledBitmapFrame(renderBitmapDisposable, this.logger);
        }

        public override void Dispose()
        {
            this.DisposeNative(true);
            base.Dispose();
            GC.SuppressFinalize(this);
        }


        private void DisposeNative(bool disposing)
        {
            this.logger.LogDebug("Disposing snapper resources, disposing = {disposing}", disposing);

            // https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool?view=aspnetcore-7.0#:~:text=When%20DefaultObjectPoolProvider%20is%20used%20and
            // render pool is SOMETIMES IDisposable
            if (disposing && this.renderPool is IDisposable disposablePool)
            {
                this.logger.LogTrace("Render pool is disposable, disposing it");
                disposablePool.Dispose();
            }

            this.renderPool = null;

            if (this.inputs != null)
            {
                for (var i = 0; i < this.inputs.Length; i++)
                {
                    if (disposing)
                    {
                        this.inputs[i]?.Dispose();
                    }

                    this.inputs[i] = null;
                }

                this.inputs = null;
            }

            this.logger.LogDebug("Disposed snapper resources, disposing = {disposing}", disposing);
        }

        private record PooledBitmapFrame(PooledWrapper<Bitmap> PooledBitmap, ILogger Logger) : IPooledFrame
        {
            public Bitmap Value => this.PooledBitmap.Value;

            public void Dispose()
            {
                this.Logger.LogTrace("Releasing bitmap frame");
                this.PooledBitmap?.Dispose();
            }
        }
    }
}
