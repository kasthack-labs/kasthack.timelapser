namespace kasthack.TimeLapser.Core.Impl.Pooling
{
    using System;

    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// Factory for object pools.
    /// </summary>
    public static class ObjectPoolFactory
    {
        public static ObjectPool<T> Create<T>(Func<T> factory, int? maxRetained = 0)
            where T : class => new DefaultObjectPoolProvider()
            {
                MaximumRetained = maxRetained ?? Environment.ProcessorCount * 2,
            }
            .Create<T>(new FactoryObjectPoolPolicy<T>(factory));

        /// <summary>
        /// Object pool policy that uses factory to create objects.
        /// </summary>
        /// <typeparam name="T">Pooled object type.</typeparam>
        /// <param name="Factory">Factory to create objects.</param>
        private record FactoryObjectPoolPolicy<T>(Func<T> Factory) : IPooledObjectPolicy<T>
        {
            public T Create() => this.Factory();

            public bool Return(T obj) => true;
        }
    }
}
