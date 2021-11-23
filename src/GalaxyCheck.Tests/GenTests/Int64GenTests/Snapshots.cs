using System.Collections.Generic;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GalaxyCheck.Gens;
using Snapshooter.Xunit;
using Snapshooter;

namespace Tests.V2.GenTests.Int64GenTests
{
    public class Snapshots
    {
        [Fact]
        public void Values()
        {
            var seeds = Enumerable.Range(0, 5);

            var gens = new Dictionary<string, IIntGen<long>>
            {
                { "Between_0_And_10", Gen.Int64().Between(0, 10) },
                { "Between_0_And_100", Gen.Int64().Between(0, 100) },
                { "Between_0_And_10000", Gen.Int64().Between(0, 10000) },
                { "Between_Default", Gen.Int64() },
            };

            var biases = new[] { Gen.Bias.None, Gen.Bias.WithSize };

            foreach (var seed in seeds)
            foreach (var (betweenDescription, gen) in gens)
            foreach (var bias in biases)
            {
                var sample = gen.WithBias(bias).Sample(seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                    betweenDescription,
                    $"Bias_{bias}",
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }

        [Fact]
        public void ExampleSpaces()
        {
            var seeds = Enumerable.Range(0, 5);

            var gens = new Dictionary<string, IIntGen<long>>
            {
                { "Between_0_And_10", Gen.Int64().Between(0, 10) },
                { "Between_0_And_100", Gen.Int64().Between(0, 100) },
            };

            var biases = new[] { Gen.Bias.None, Gen.Bias.WithSize };

            foreach (var seed in seeds)
            foreach (var (betweenDescription, gen) in gens)
            foreach (var bias in biases)
            {
                var sample = gen
                    .WithBias(bias)
                    .Advanced.SampleOneExampleSpace(seed: seed, size: 75)
                    .Take(500)
                    .Render(x => x.ToString());

                var nameExtension = string.Join("_", new[]
                {
                    betweenDescription,
                    $"Bias_{bias}",
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
