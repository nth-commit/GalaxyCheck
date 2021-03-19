using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Internal.Sizing;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public record GenParameters(IRng Rng, Size Size)
    {
        public GenParameters With(
            IRng? rng = null,
            Size? size = null) =>
                new GenParameters(rng ?? Rng, size ?? Size);
    }

    public interface IGenAdvanced
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
        /// <param name="parameters">The initial parameters which the generator is seeded with.</param>
        /// are.</param>
        /// <returns>An infinite enumerable of generated iterations.</returns>
        IEnumerable<IGenIteration> Run(GenParameters parameters);
    }

    /// <summary>
    /// A random data generator.
    /// </summary>
    public interface IGen
    {
        IGenAdvanced Advanced { get; }
    }

    public interface IGenAdvanced<out T> : IGenAdvanced
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
        /// <param name="parameters">The initial parameters which the generator is seeded with.</param>
        /// are.</param>
        /// <returns>An infinite enumerable of generated iterations.</returns>
        new IEnumerable<IGenIteration<T>> Run(GenParameters parameters);
    }

    /// <summary>
    /// A random data generator.
    /// </summary>
    /// <typeparam name="T">The type of the generator's values.</typeparam>
    public interface IGen<out T> : IGen
    {
        new IGenAdvanced<T> Advanced { get; }
    }
}
