﻿using GalaxyCheck.Internal.Utility;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Random;
using GalaxyCheck.Internal.ExampleSpaces;
using System.Collections.Generic;
using System.Linq;
using GalaxyCheck.Internal.GenIterations;

namespace GalaxyCheck
{
    public static class SampleExtensions
    {
        /// <summary>
        /// Enumerates a generator to produce a sample of it's values.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to sample.</param>
        /// <param name="config">An optional configuration for the sample.</param>
        /// <returns>A list of values produced by the generator.</returns>
        public static List<T> Sample<T>(
            this IGen<T> gen,
            int? iterations = null,
            int? seed = null,
            int? size = null) =>
                gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size).Values;

        public static T SampleOne<T>(
            this IGen<T> gen,
            int? seed = null,
            int? size = null) => gen.Sample(iterations: 1, seed: seed, size: size).Single();

        public record SampleWithMetricsResult<T>(
            List<T> Values,
            int RandomnessConsumption);

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var exampleSpacesSample = advanced.SampleExampleSpacesWithMetrics(
                iterations: iterations,
                seed: seed,
                size: size);

            return new SampleWithMetricsResult<T>(
                exampleSpacesSample.Values.Select(exampleSpace => exampleSpace.Current.Value).ToList(),
                exampleSpacesSample.RandomnessConsumption);
        }

        public static List<ExampleSpace<T>> SampleExampleSpaces<T>(
            this IGenAdvanced<T> gen,
            int? iterations = null,
            int? seed = null,
            int? size = null) =>
                gen.SampleExampleSpacesWithMetrics(
                    iterations: iterations,
                    seed: seed,
                    size: size).Values;

        public static ExampleSpace<T> SampleOneExampleSpace<T>(
            this IGenAdvanced<T> gen,
            int? seed = null,
            int? size = null) => gen.SampleExampleSpaces(iterations: 1, seed: seed, size: size).Single();

        public static SampleWithMetricsResult<ExampleSpace<T>> SampleExampleSpacesWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var initialRng = seed == null ? Rng.Spawn() : Rng.Create(seed.Value);
            var initialSize = size == null ? new Size(50) : new Size(size.Value);

            var instances = advanced.Sample(initialRng, initialSize).Take(iterations ?? 100).ToList();

            var exampleSpaces = instances.Select(instance => instance.ExampleSpace).ToList();
            var nextRng = instances.LastOrDefault()?.NextRng ?? initialRng;
            var randomnessConsumption = nextRng.Order - initialRng.Order;

            return new SampleWithMetricsResult<ExampleSpace<T>>(exampleSpaces, randomnessConsumption);
        }

        private static IEnumerable<GenInstance<T>> Sample<T>(this IGenAdvanced<T> advanced, IRng rng, Size size)
        {
            var stream = advanced.Run(rng, size).WithDiscardCircuitBreaker(iteration => iteration is GenDiscard<T>);

            foreach (var iteration in stream)
            {
                var valueOption = iteration.Match<T, Option<GenInstance<T>>>(
                    onInstance: instance => new Option.Some<GenInstance<T>>(instance),
                    onDiscard: discard => new Option.None<GenInstance<T>>(),
                    onError: error => throw new Exceptions.GenErrorException(error.GenName, error.Message));

                if (valueOption is Option.Some<GenInstance<T>> valueSome)
                {
                    yield return valueSome.Value;
                }
            }
        }
    }
}
