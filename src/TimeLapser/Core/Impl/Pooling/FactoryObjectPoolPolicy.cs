namespace kasthack.TimeLapser.Core.Impl.Pooling
{
    using System;

    using Microsoft.Extensions.ObjectPool;

    /// <summary>
    /// Object pool policy that uses factory to create objects.
    /// </summary>
    /// <typeparam name="T">Pooled object type.</typeparam>
    /// <param name="Factory">Factory to create objects.</param>
    public record FactoryObjectPoolPolicy<T>(Func<T> Factory) : IPooledObjectPolicy<T>
    {
        public T Create() => this.Factory();

        public bool Return(T obj) => true;
    }
}
