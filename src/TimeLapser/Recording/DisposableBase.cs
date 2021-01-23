namespace kasthack.TimeLapser
{
    using System;

    public abstract class DisposableBase : IDisposable
    {
        private bool disposed = false;

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);
            this.disposed = true;
        }

        protected bool ThrowIfDisposed() => this.disposed ? throw new ObjectDisposedException(this.GetType().Name) : true;
    }
}
