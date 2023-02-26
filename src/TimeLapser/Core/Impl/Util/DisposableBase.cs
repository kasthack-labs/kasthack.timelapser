namespace kasthack.TimeLapser.Core.Impl.Util
{
    using System;

    /// <summary>
    /// Base class for disposable objects.
    /// </summary>
    public abstract class DisposableBase : IDisposable
    {
        protected bool Disposed { get; private set; }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            this.Disposed = true;
        }

        protected bool ThrowIfDisposed() => this.Disposed ? throw new ObjectDisposedException(this.GetType().Name) : true;
    }
}
