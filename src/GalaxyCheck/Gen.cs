using GalaxyCheck.Abstractions;
using GalaxyCheck.Aggregators;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static class Gen
    {
        /// <summary>
        /// A flag which indicates how values produced by generators should scale with respect to the size parameter.
        /// </summary>
        public enum Bias
        {
            /// <summary>
            /// Generated values should not be biased by the size parameter.
            /// </summary>
            None = 0,

            /// <summary>
            /// Generated values should scale linearly with the size parameter.
            /// </summary>
            Linear = 1
        }

        /// <summary>
        /// Creates a generator that always produces the given value.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="value">The constant value the generator should produce.</param>
        /// <returns>The new generator.</returns>
        public static IGen<T> Constant<T>(T value) => new PrimitiveGen<T>(
            (useNextInt, size) => value,
            ShrinkFunc.None<T>(),
            MeasureFunc.Unmeasured<T>());

        /// <summary>
        /// Creates a generator that produces 32-bit integers. By default, it will generate integers in the full range
        /// (-2,147,483,648 to 2,147,483,648), but the generator returned contains configuration methods to constrain
        /// the produced integers further.
        /// </summary>
        /// <returns></returns>
        public static IInt32Gen Int32() => new Int32Gen();

        /// <summary>
        /// Projects each element in a generator to a new form.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <typeparam name="U">The type of the resultant generator's value.</typeparam>
        /// <param name="gen">The generator to apply the projection to.</param>
        /// <param name="selector">A transform function to apply to each value.</param>
        /// <returns>A new generator with the projection applied.</returns>
        public static IGen<U> Select<T, U>(this IGen<T> gen, Func<T, U> selector) => gen.Transform(stream =>
        {
            return stream.Select(iteration => iteration.Match<T, GenIteration<U>>(
                onInstance: (instance) => new GenInstance<U>(instance.InitialRng, instance.NextRng, instance.ExampleSpace.Select(selector)),
                onError: (error) => new GenError<U>(error.InitialRng, error.NextRng, error.GenName, error.Message)));
        });

        internal static IGen<U> Transform<T, U>(
            this IGen<T> gen,
            Func<IEnumerable<GenIteration<T>>, IEnumerable<GenIteration<U>>> transformer) =>
                new FunctionGen<U>((rng, size) => transformer(gen.Run(rng, size)));
    }
}
