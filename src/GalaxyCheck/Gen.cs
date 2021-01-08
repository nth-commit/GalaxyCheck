using GalaxyCheck.Abstractions;
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
        /// (-2,147,483,648 to 2,147,483,647), but the generator returned contains configuration methods to constrain
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
            return stream.Select(iteration =>
            {
                var iterationBuilder = GenIterationBuilder.FromIteration(iteration);

                return iteration.Match<T, GenIteration<U>>(
                    onInstance: (instance) => iterationBuilder.ToInstance(instance.ExampleSpace.Select(selector)),
                    onError: (error) => iterationBuilder.ToError<U>(error.GenName, error.Message));
            });
        });

        /// <summary>
        /// Converts a generator into a property the supplied property function.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to convert to a property.</param>
        /// <param name="func">The property function.</param>
        /// <returns>The property comprised of the generator and the supplied property function.</returns>
        public static IProperty<T> ToProperty<T>(this IGen<T> gen, Func<T, bool> func) => Property.ForAll(gen, func);

        private static IGen<U> Transform<T, U>(
            this IGen<T> gen,
            Func<IEnumerable<GenIteration<T>>, IEnumerable<GenIteration<U>>> transformer) =>
                new FunctionGen<U>((rng, size) => transformer(gen.Advanced.Run(rng, size)));
    }
}
