namespace kasthack.TimeLapser.Core.Impl.Pooling
{
    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// Extension methods for object pool.
    /// </summary>
    public static class PoolExtensions
    {
        /// <summary>
        /// Gets disposable wrapper for pooled object.
        /// </summary>
        /// <typeparam name="T">Object type.</typeparam>
        /// <param name="pool">Pool to use.</param>
        /// <returns>Disposable wrapper.</returns>
        public static PooledWrapper<T> GetDisposable<T>(this ObjectPool<T> pool)
            where T : class
        {
            return new PooledWrapper<T>(pool.Get(), pool);
        }
    }
}
