using GalaxyCheck.Abstractions;
using GalaxyCheck.Random;
using GalaxyCheck.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Runners
{
    public record SampleOptions
    {
        public static SampleOptions Create() => new SampleOptions(iterations: 100, seed: null);

        public int Iterations { get; init; }

        public int? Seed { get; init; }

        private SampleOptions(int iterations, int? seed)
        {
            Iterations = iterations;
            Seed = seed;
        }

        public SampleOptions WithSeed(int seed) => new SampleOptions(
            iterations: Iterations,
            seed: seed);
    }

    public static class GenSampleWithMetricsExtensions
    {
        public record SampleWithMetricsResult<T>(
            List<T> Values,
            int RandomConsumptionCount);

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGen<T> gen,
            Func<SampleOptions, SampleOptions>? configure = null)
        {
            var defaultOptions = SampleOptions.Create();
            var options = configure == null ? defaultOptions : configure.Invoke(defaultOptions);
            var rng = options.Seed == null ? Rng.Spawn() : Rng.Create(options.Seed.Value);

            var instances = Sample(gen, rng).Take(options.Iterations).ToList();

            var values = instances.Select(instance => instance.Value).ToList();
            var randomnessCount = 0; // TODO: Infer from instance metadata

            return new SampleWithMetricsResult<T>(values, randomnessCount);
        }

        private static IEnumerable<GenInstance<T>> Sample<T>(IGen<T> gen, IRng rng)
        {
            foreach (var iteration in gen.Run(rng))
            {
                var valueOption = iteration.Match<T, Option<GenInstance<T>>>(
                    instance => new Option.Some<GenInstance<T>>(instance));

                if (valueOption is Option.Some<GenInstance<T>> valueSome)
                {
                    yield return valueSome.Value;
                }
            }
        }
    }
}
