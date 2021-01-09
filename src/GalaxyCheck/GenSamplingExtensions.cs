using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Exceptions;
using GalaxyCheck.Sizing;
using GalaxyCheck.Utility;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static class GenSamplingExtensions
    {
        /// <summary>
        /// Enumerates a generator to produce a sample of it's values.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <param name="gen">The generator to sample.</param>
        /// <param name="config">An optional configuration for the sample.</param>
        /// <returns>A list of values produced by the generator.</returns>
        public static List<T> Sample<T>(this IGen<T> gen, RunConfig? config = null) =>
            gen.Advanced.SampleWithMetrics(config).Values;
    }

    public static class GenAdvancedSamplingExtensions
    {
        public record SampleWithMetricsResult<T>(
            List<T> Values,
            int RandomnessConsumption);

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            RunConfig? config = null)
        {
            var exampleSpacesSample = advanced.SampleExampleSpacesWithMetrics(config);

            return new SampleWithMetricsResult<T>(
                exampleSpacesSample.Values.Select(exampleSpace => exampleSpace.Current.Value).ToList(),
                exampleSpacesSample.RandomnessConsumption);
        }

        public static List<ExampleSpace<T>> SampleExampleSpaces<T>(this IGenAdvanced<T> gen, RunConfig? config = null) =>
            gen.SampleExampleSpacesWithMetrics(config).Values;

        public static SampleWithMetricsResult<ExampleSpace<T>> SampleExampleSpacesWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            RunConfig? config = null)
        {
            config ??= new RunConfig();
            var rng = config.Rng;
            var iterations = config.Iterations ?? 100;
            var size = config.Size ?? new Size(50);

            var instances = advanced.Sample(rng, size).Take(iterations).ToList();

            var exampleSpaces = instances.Select(instance => instance.ExampleSpace).ToList();
            var nextRng = instances.Last().NextRng;
            var randomnessConsumption = nextRng.Order - rng.Order;

            return new SampleWithMetricsResult<ExampleSpace<T>>(exampleSpaces, randomnessConsumption);
        }

        private static IEnumerable<GenInstance<T>> Sample<T>(this IGenAdvanced<T> advanced, IRng rng, Size size)
        {
            var stream = advanced
                .Run(rng, size)
                .WithConsecutiveDiscardCount()
                .Select(x =>
                {
                    if (x.consecutiveDiscards > 1000)
                    {
                        throw new GenExhaustionException();
                    }

                    return x.iteration;
                });

            foreach (var iteration in stream)
            {
                var valueOption = iteration.Match<T, Option<GenInstance<T>>>(
                    onInstance: instance => new Option.Some<GenInstance<T>>(instance),
                    onDiscard: discard => new Option.None<GenInstance<T>>(),
                    onError: error => throw new GenErrorException(error.GenName, error.Message));

                if (valueOption is Option.Some<GenInstance<T>> valueSome)
                {
                    yield return valueSome.Value;
                }
            }
        }
    }
}
