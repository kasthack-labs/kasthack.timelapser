using System;

namespace TimeLapser {
    public abstract class DisposableBase : IDisposable {
        private bool _disposed = false;
        protected bool ThrowIfDisposed() => _disposed ? throw new ObjectDisposedException(nameof(SDGSnapper)) : true;
        public virtual void Dispose() => _disposed = true;
    }
}