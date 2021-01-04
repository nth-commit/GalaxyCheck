using GalaxyCheck.Abstractions;
using System;
using System.Linq;

namespace GalaxyCheck.Aggregators
{
    public record MinimalOptions
    {
        public static MinimalOptions Create() => new MinimalOptions(seed: null);

        public int? Seed { get; init; }

        private MinimalOptions(int? seed)
        {
            Seed = seed;
        }

        public MinimalOptions WithSeed(int seed) => new MinimalOptions(seed: seed);
    }

    public static class GenMinimalizingExtensions
    {
        public static T Minimal<T>(
            this IGen<T> gen,
            Func<MinimalOptions, MinimalOptions>? configure = null)
        {
            var defaultOptions = MinimalOptions.Create();
            var options = configure == null ? defaultOptions : configure.Invoke(defaultOptions);

            var sample = gen.SampleExampleSpaces(opts => options.Seed == null
                ? opts.WithIterations(1)
                : opts.WithIterations(1).WithSeed(options.Seed.Value));

            return sample.First().Minimal()!.Value;
        }
    }
}
