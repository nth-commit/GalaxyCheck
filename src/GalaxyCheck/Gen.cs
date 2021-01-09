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
        /// <typeparam name="TResult">The type of the resultant generator's value.</typeparam>
        /// <param name="gen">The generator to apply the projection to.</param>
        /// <param name="selector">A transform function to apply to each value.</param>
        /// <returns>A new generator with the projection applied.</returns>
        public static IGen<TResult> Select<T, TResult>(this IGen<T> gen, Func<T, TResult> selector)
        {
            return gen.Transform(stream => stream.Select(iteration =>
            {
                var iterationBuilder = GenIterationBuilder.FromIteration(iteration);

                return iteration.Match<T, GenIteration<TResult>>(
                    onInstance: (instance) => iterationBuilder.ToInstance(instance.ExampleSpace.Select(selector)),
                    onDiscard: (discard) => iterationBuilder.ToDiscard<TResult>(),
                    onError: (error) => iterationBuilder.ToError<TResult>(error.GenName, error.Message));
            }));
        }

        /// <summary>
        /// Filters a generator's values based on a predicate. Does not alter the structure of the stream. Instead,
        /// it replaces the filtered value with a token, which enables discard counting whilst running the generator.
        /// </summary>
        /// <typeparam name="T">The type of the genreator's value.</typeparam>
        /// <param name="gen">The generator to apply the predicate to.</param>
        /// <param name="pred">A predicate function that tests each value.</param>
        /// <returns>A new generator with the filter applied.</returns>
        public static IGen<T> Where<T>(this IGen<T> gen, Func<T, bool> pred) => gen.Transform(stream =>
        {
            return stream.Select(iteration => iteration.Match<T, GenIteration<T>>(
                onInstance: instance =>
                {
                    var iterationBuilder = GenIterationBuilder.FromIteration(iteration);
                    var filteredExampleSpace = instance.ExampleSpace.Where(pred);
                    return filteredExampleSpace.Any()
                        ? iterationBuilder.ToInstance(filteredExampleSpace)
                        : iterationBuilder.ToDiscard<T>();
                },
                onDiscard: discard => discard,
                onError: error => error));
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
