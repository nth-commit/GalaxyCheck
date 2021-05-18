using GalaxyCheck;
using GalaxyCheck.Gens;
using Snapshooter;
using Snapshooter.Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Tests.V2.GenTests.ListGenTests
{
    public class Snapshots
    {
        [Fact]
        public void Values()
        {
            var gens = new Dictionary<string, IListGen<int>>
            {
                { "Between_Counts_1_And_4_And_Elements_0_And_10", Gen.Int32().Between(0, 10).ListOf().WithCountBetween(1, 4) },
                { "Between_Counts_1_And_8_And_Elements_0_And_100", Gen.Int32().Between(0, 100).ListOf().WithCountBetween(1, 8) },
                { "Between_Counts_1_And_16_And_Elements_0_And_10000", Gen.Int32().Between(0, 10000).ListOf().WithCountBetween(1, 16) },
                { "Between_Defaults", Gen.Int32().ListOf() },
            };

            var biases = new[] { Gen.Bias.None, Gen.Bias.WithSize };

            foreach (var (betweenDescription, gen) in gens)
            foreach (var bias in biases)
            {
                var gen0 = gen.WithCountBias(bias);

                var sample = gen.Sample(seed: 0);

                var nameExtension = string.Join("_", new[]
                {
                    betweenDescription,
                    $"Bias_{bias}",
                    $"Seed_{0}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }

        [Fact]
        public void ExampleSpaces_Int32()
        {
            var seeds = Enumerable.Range(0, 3);

            var gens = new Dictionary<string, IListGen<int>>
            {
                { "Between_Counts_1_And_4_And_Elements_0_And_10", Gen.Int32().Between(0, 10).ListOf().WithCountBetween(1, 4) },
                { "Between_Counts_1_And_8_And_Elements_0_And_100", Gen.Int32().Between(0, 100).ListOf().WithCountBetween(1, 8) },
            };

            var biases = new[] { Gen.Bias.None, Gen.Bias.WithSize };

            foreach (var seed in seeds)
            foreach (var (betweenDescription, gen) in gens)
            foreach (var bias in biases)
            {
                var gen0 = gen.WithCountBias(bias);

                var sample = gen.Advanced
                    .SampleOneExampleSpace(seed: seed, size: 75)
                    .Take(500)
                    .Render(x => JsonSerializer.Serialize(x));

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
