using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Utility;
using System;
using System.Linq;

namespace GalaxyCheck
{
    public static class WhereExtensions
    {
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
                var filteredExampleSpace = instance.ExampleSpace.Filter(pred);
                return filteredExampleSpace == null
                    ? iterationBuilder.ToDiscard<T>()
                    : iterationBuilder.ToInstance(filteredExampleSpace);
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

            return gen
                .TransformInstances(applyPredicateToInstance)
                .TransformStream(resizeAndTerminateAfterConsecutiveDiscards)
                .Transform(GenTransformations.Repeat<T>());
        }
    }
}
