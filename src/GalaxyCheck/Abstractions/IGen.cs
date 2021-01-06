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
        /// A container for the advanced functionality of generators.
        /// </summary>
        IGenAdvanced<T> Advanced { get; }
    }

    public interface IGenAdvanced<T>
    {
        /// <summary>
        /// Creates an infinite enumerable for this generator, given some initial generation parameters.
        /// 
        /// To use this API, you must know what a GenIteration is and how to handle each case. Some of these cases are
        /// reasonably complex, and handled by the runners. For example, it's possible to accidentally create a
        /// generator that spins forever without ever generating a value (i.e. if an impossible where clause was added
        /// to it). The built-in runners know how to short-circuit this situation.
        /// 
        /// Instead, it is recommended to use `Sample()` or `Minimal()`.
        /// </summary>
        /// <param name="rng">The initial RNG to seed the generator with.</param>
        /// <param name="size">The initial size to run the generator with. Determines how large the generated values
        /// are.</param>
        /// <returns>An infinite enumerable of generated iterations.</returns>
        IEnumerable<GenIteration<T>> Run(IRng rng, ISize size);
    }

    public abstract record GenIteration<T>
    {
        public IRng InitialRng { get; init; }

        public IRng NextRng { get; init; }

        internal GenIteration(IRng initialRng, IRng nextRng)
        {
            InitialRng = initialRng;
            NextRng = nextRng;
        }
    }

    public sealed record GenInstance<T>(IRng InitialRng, IRng NextRng, IExampleSpace<T> ExampleSpace)
        : GenIteration<T>(InitialRng, NextRng);

    public sealed record GenError<T>(IRng InitialRng, IRng NextRng, string GenName, string Message)
        : GenIteration<T>(InitialRng, NextRng);

    public static class GenIterationExtensions
    {
        public static TResult Match<T, TResult>(
            this GenIteration<T> iteration,
            Func<GenInstance<T>, TResult> onInstance,
            Func<GenError<T>, TResult> onError)
        {
            return iteration switch
            {
                GenInstance<T> instance => onInstance(instance),
                GenError<T> error => onError(error),
                _ => throw new NotSupportedException()
            };
        }
    }
}
