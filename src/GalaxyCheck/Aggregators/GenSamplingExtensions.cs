using GalaxyCheck.Abstractions;
using GalaxyCheck.Gens;
using GalaxyCheck.Random;
using GalaxyCheck.Sizing;
using GalaxyCheck.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Aggregators
{
    public record SampleOptions
    {
        public static SampleOptions Create() => new SampleOptions(iterations: 100, seed: null, size: new Size(50));

        public int Iterations { get; init; }

        public int? Seed { get; init; }

        public ISize Size { get; init; }

        private SampleOptions(int iterations, int? seed, ISize size)
        {
            Iterations = iterations;
            Seed = seed;
            Size = size;
        }

        public SampleOptions WithIterations(int iterations) => new SampleOptions(
            iterations: iterations,
            seed: Seed,
            size: Size);

        public SampleOptions WithSeed(int seed) => new SampleOptions(
            iterations: Iterations,
            seed: seed,
            size: Size);

        public SampleOptions WithSize(ISize size) => new SampleOptions(
            iterations: Iterations,
            seed: Seed,
            size: size);
    }

    public static class GenSamplingExtensions
    {
        public record SampleWithMetricsResult<T>(
            List<T> Values,
            int RandomnessConsumption);

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGen<T> gen,
            Func<SampleOptions, SampleOptions>? configure = null)
        {
            var exampleSpacesSample = gen.SampleExampleSpacesWithMetrics(configure);

            return new SampleWithMetricsResult<T>(
                exampleSpacesSample.Values.Select(exampleSpace => exampleSpace.Traverse().First().Value).ToList(),
                exampleSpacesSample.RandomnessConsumption);
        }

        public static List<IExampleSpace<T>> SampleExampleSpaces<T>(
            this IGen<T> gen,
            Func<SampleOptions, SampleOptions>? configure = null) =>
                gen.SampleExampleSpacesWithMetrics(configure).Values;

        public static SampleWithMetricsResult<IExampleSpace<T>> SampleExampleSpacesWithMetrics<T>(
            this IGen<T> gen,
            Func<SampleOptions, SampleOptions>? configure = null)
        {
            var defaultOptions = SampleOptions.Create();
            var options = configure == null ? defaultOptions : configure.Invoke(defaultOptions);
            var rng = options.Seed == null ? Rng.Spawn() : Rng.Create(options.Seed.Value);

            var instances = Sample(gen, rng, options.Size).Take(options.Iterations).ToList();

            var exampleSpaces = instances.Select(instance => instance.ExampleSpace).ToList();
            var nextRng = instances.Last().NextRng;
            var randomnessConsumption = nextRng.Order - rng.Order;

            return new SampleWithMetricsResult<IExampleSpace<T>>(exampleSpaces, randomnessConsumption);
        }

        private static IEnumerable<GenInstance<T>> Sample<T>(IGen<T> gen, IRng rng, ISize size)
        {
            foreach (var iteration in gen.Run(rng, size))
            {
                var valueOption = iteration.Match<T, Option<GenInstance<T>>>(
                    instance => new Option.Some<GenInstance<T>>(instance),
                    error => throw new GenErrorException(error.GenName, error.Message));

                if (valueOption is Option.Some<GenInstance<T>> valueSome)
                {
                    yield return valueSome.Value;
                }
            }
        }
    }
}
