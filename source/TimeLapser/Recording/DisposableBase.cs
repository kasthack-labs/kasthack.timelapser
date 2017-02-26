using System;

namespace TimeLapser {
    public abstract class DisposableBase : IDisposable {
        private bool _disposed = false;
        protected bool ThrowIfDisposed() {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
            return true;
        }

        public virtual void Dispose() => _disposed = true;
    }
}