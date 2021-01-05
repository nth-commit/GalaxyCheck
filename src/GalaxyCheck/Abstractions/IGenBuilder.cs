using System;

namespace GalaxyCheck.Abstractions
{
    /// <summary>
    /// A random data generator augmented with operators that allow transformation of the generated iterations.
    /// </summary>
    /// <typeparam name="T">The type of the generator's values.</typeparam>
    public interface IGenBuilder<T> : IGen<T>
    {
        IGenBuilder<U> Select<U>(Func<T, U> selector);
    }
}
