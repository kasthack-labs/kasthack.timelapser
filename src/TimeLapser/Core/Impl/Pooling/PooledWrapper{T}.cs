namespace kasthack.TimeLapser.Core.Impl.Pooling
{
    using System;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// Wrapper for pooled objects, that returns them to pool on dispose.
    /// </summary>
    /// <typeparam name="T">Pooled object type.</typeparam>
    public class PooledWrapper<T> : IDisposable
        where T : class
    {
        private readonly ILogger logger;

        private bool disposed = false;
        private T value;
        private ObjectPool<T> holdingPool;

        public PooledWrapper(T value, ObjectPool<T> holdingPool, ILogger logger)
        {
            this.Value = value;
            this.HoldingPool = holdingPool;
            this.logger = logger;
        }

        ~PooledWrapper()
        {
            this.Dispose(false);
        }

        public T Value
        {
            get
            {
                this.ThrowIfDisposed();
                return this.value;
            }
            private set => this.value = value;
        }

        private ObjectPool<T> HoldingPool
        {
            get
            {
                this.ThrowIfDisposed();
                return this.holdingPool;
            }

            set
            {
                this.ThrowIfDisposed();
                this.holdingPool = value;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(PooledWrapper<T>));
            }
        }

        private void Dispose(bool disposing)
        {
            if (this.disposed)
            {
                return;
            }

            if (disposing)
            {
                this.logger.LogTrace("Returning value to pool");
                this.HoldingPool.Return(this.Value);
            }

            this.Value = null;
            this.HoldingPool = null;
            this.disposed = true;
        }
    }
}
