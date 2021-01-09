using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens;
using GalaxyCheck.Utility;
using System;
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
        public static IGen<T> Constant<T>(T value) => Advanced.Create((useNextInt, size) => value);

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
            GenInstanceTransformation<T, TResult> transformation = (instance) => GenIterationBuilder
                .FromIteration(instance)
                .ToInstance(instance.ExampleSpace.Select(selector));

            return gen.TransformInstances(transformation);
        }

        /// <summary>
        /// Filters a generator's values based on a predicate. Does not alter the structure of the stream. Instead,
        /// it replaces the filtered value with a token, which enables discard counting whilst running the generator.
        /// </summary>
        /// <typeparam name="T">The type of the genreator's value.</typeparam>
        /// <param name="gen">The generator to apply the predicate to.</param>
        /// <param name="pred">A predicate function that tests each value.</param>
        /// <returns>A new generator with the filter applied.</returns>
        public static IGen<T> Where<T>(this IGen<T> gen, Func<T, bool> pred)
        {
            GenInstanceTransformation<T, T> applyPredicateToInstance = (instance) =>
            {
                var iterationBuilder = GenIterationBuilder.FromIteration(instance);
                var filteredExampleSpace = instance.ExampleSpace.Where(pred);
                return filteredExampleSpace.Any()
                    ? iterationBuilder.ToInstance(filteredExampleSpace)
                    : iterationBuilder.ToDiscard<T>();
            };

            GenStreamTransformation<T, T> resizeAndTerminateAfterConsecutiveDiscards = (stream) =>
            {
                const int MaxConsecutiveDiscards = 10;
                return stream
                    .WithConsecutiveDiscardCount()
                    .Select((x) =>
                    {
                        if (x.consecutiveDiscards >= MaxConsecutiveDiscards)
                        {
                            var resizedIteration = GenIterationBuilder
                                .FromIteration(x.iteration)
                                .WithNextSize(x.iteration.NextSize.BigIncrement())
                                .ToDiscard<T>();
                            return (resizedIteration, x.consecutiveDiscards);
                        }
                        else
                        {
                            return (x.iteration, x.consecutiveDiscards);
                        }
                    })
                    .TakeWhileInclusive((x) => x.consecutiveDiscards < MaxConsecutiveDiscards)
                    .Select(x => x.iteration);
            };

            GenTransformation<T, T> repeat = (IGen<T> gen) => new FunctionGen<T>((rng, size) =>
            {
                 return EnumerableExtensions
                     .Repeat(() => gen.Advanced.Run(rng, size))
                     .Tap((iteration) =>
                     {
                         rng = iteration.NextRng;
                         size = iteration.NextSize;
                     });
             });

            return gen
                .TransformInstances(applyPredicateToInstance)
                .TransformStream(resizeAndTerminateAfterConsecutiveDiscards)
                .Transform(repeat);
        }

        /// <summary>
        /// Converts a generator into a property the supplied property function.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to convert to a property.</param>
        /// <param name="func">The property function.</param>
        /// <returns>The property comprised of the generator and the supplied property function.</returns>
        public static IProperty<T> ToProperty<T>(this IGen<T> gen, Func<T, bool> func) => Property.ForAll(gen, func);

        public static class Advanced
        {
            public static IGen<T> Create<T>(
                StatefulGenFunc<T> generate,
                ShrinkFunc<T>? shrink = null,
                MeasureFunc<T>? measure = null) => new PrimitiveGen<T>(
                    generate,
                    shrink ?? ShrinkFunc.None<T>(),
                    measure ?? MeasureFunc.Unmeasured<T>());
        }
    }
}
