using System;
using System.Collections.Generic;

namespace GalaxyCheck.Abstractions
{
    /// <summary>
    /// A random data generator.
    /// </summary>
    /// <typeparam name="T">The type of the generator's values.</typeparam>
    public interface IGen<T>
    {
        /// <summary>
        /// Creates an infinite enumerable for this generator, given some initial generation parameters.
        /// 
        /// Advanced use only; To use this API, you must know what a GenIteration is and how to handle each case. Some
        /// of these cases are reasonably complex, and handled by the runners. For example, it's possible to
        /// accidentally create a generator that spins forever without ever generating a value (i.e. if an impossible
        /// where clause was added to it). The built-in runners know how to short-circuit this situation.
        /// 
        /// Instead, use `Sample()` or `Minimal()`.
        /// </summary>
        /// <param name="rng">The initial RNG to seed the generator with.</param>
        /// <returns>An infinite enumerable of generated iterations.</returns>
        IEnumerable<GenIteration<T>> Run(IRng rng);
    }

    public abstract record GenIteration<T>
    {
        internal GenIteration()
        {
        }
    }

    public sealed record GenInstance<T>(IExampleSpace<T> ExampleSpace) : GenIteration<T>;

    public static class GenIterationExtensions
    {
        public static TResult Match<T, TResult>(
            this GenIteration<T> iteration,
            Func<GenInstance<T>, TResult> onInstance)
        {
            return iteration switch
            {
                GenInstance<T> instance => onInstance(instance),
                _ => throw new NotSupportedException()
            };
        }
    }
}
