﻿using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using System;
using System.Linq;
using GalaxyCheck.Gens.Iterations.Generic;

namespace GalaxyCheck
{
    public static partial class Extensions
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
                    return GenIterationFactory.Discard<T>(
                        instance.ReplayParameters,
                        instance.NextParameters,
                        instance.ExampleSpace);
                }

                return GenIterationFactory.Instance(
                    instance.ReplayParameters,
                    instance.NextParameters,
                    filteredExampleSpace);
            };

            GenStreamTransformation<T, T> resizeAndTerminateAfterConsecutiveDiscards = (stream) =>
            {
                const int MaxConsecutiveDiscards = 5;

                return stream
                    .WithConsecutiveDiscardCount(_ => true, iteration => iteration.IsDiscard())
                    .Select((x) =>
                    {
                        var (iteration, consecutiveDiscardCount) = x;

                        if (consecutiveDiscardCount >= MaxConsecutiveDiscards)
                        {
                            var resizedIteration = GenIterationFactory.Discard<T>(
                                iteration.ReplayParameters,
                                iteration.Data.Discard!.NextParameters with
                                {
                                    Size = iteration.Data.Discard!.NextParameters.Size.BigIncrement()
                                },
                                iteration.Data.Discard!.ExampleSpace);

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

            // TODO: This should take bigger and bigger jumps if consecutive discards continue to accumulate, even between sizes
            return gen
                .TransformInstances(applyPredicateToInstance)
                .TransformStream(resizeAndTerminateAfterConsecutiveDiscards)
                .Repeat();
        }
    }
}
