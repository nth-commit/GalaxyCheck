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
        /// <param name="gen">The generator to apply the predicate to.</param>
        /// <param name="pred">A predicate function that tests each value.</param>
        /// <returns>A new generator with the filter applied.</returns>
        public static IGen<T> Where<T>(this IGen<T> gen, Func<T, bool> pred)
        {
            GenInstanceTransformation<T, T> applyPredicateToInstance = (instance) =>
            {
                var filteredExampleSpace = instance.ExampleSpace.Filter(pred);
                if (filteredExampleSpace == null)
                {
                    return GenIterationFactory.Discard<T>(instance.RepeatParameters, instance.NextParameters);
                }

                return GenIterationFactory.Instance(instance.RepeatParameters, instance.NextParameters, filteredExampleSpace);
            };

            GenStreamTransformation<T, T> resizeAndTerminateAfterConsecutiveDiscards = (stream) =>
            {
                const int MaxConsecutiveDiscards = 10;

                return stream
                    .WithConsecutiveDiscardCount(iteration => iteration.IsDiscard())
                    .Select((x) =>
                    {
                        var (iteration, consecutiveDiscardCount) = x;

                        if (consecutiveDiscardCount >= MaxConsecutiveDiscards)
                        {
                            var resizedIteration = GenIterationFactory.Discard<T>(
                                iteration.RepeatParameters,
                                new GenParameters(iteration.NextParameters.Rng, iteration.NextParameters.Size.BigIncrement()));

                            return (iteration: resizedIteration, consecutiveDiscardCount);
                        }
                        else
                        {
                            return (iteration, consecutiveDiscardCount);
                        }
                    })
                    .TakeWhileInclusive((x) => x.consecutiveDiscardCount < MaxConsecutiveDiscards)
                    .Select(x => x.iteration);
            };

            return gen
                .TransformInstances(applyPredicateToInstance)
                .TransformStream(resizeAndTerminateAfterConsecutiveDiscards)
                .Repeat();
        }
    }
}
