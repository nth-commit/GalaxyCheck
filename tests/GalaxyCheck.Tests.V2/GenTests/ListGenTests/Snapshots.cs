using GalaxyCheck;
using GalaxyCheck.Gens;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Tests.V2.GenTests.ListGenTests
{
    public class Snapshots
    {
        [Fact]
        public void Values()
        {
            var seeds = Enumerable.Range(0, 5);

            var gens = new Dictionary<string, IListGen<int>>
            {
                { "Between_Lengths_1_And_4_And_Elements_0_And_10", Gen.Int32().Between(0, 10).ListOf().BetweenLengths(1, 4) },
                { "Between_Lengths_1_And_8_And_Elements_0_And_100", Gen.Int32().Between(0, 100).ListOf().BetweenLengths(1, 8) },
                { "Between_Lengths_1_And_16_And_Elements_0_And_10000", Gen.Int32().Between(0, 10000).ListOf().BetweenLengths(1, 16) },
                { "Between_Defaults", Gen.Int32().ListOf() },
            };

            var biases = new[] { Gen.Bias.None, Gen.Bias.WithSize };

            foreach (var seed in seeds)
            foreach (var (betweenDescription, gen) in gens)
            foreach (var bias in biases)
            {
                var gen0 = gen.WithLengthBias(bias);

                var sample = gen.Sample(seed: seed);

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

            var gens = new Dictionary<string, IListGen<int>>
            {
                { "Between_Lengths_1_And_4_And_Elements_0_And_10", Gen.Int32().Between(0, 10).ListOf().BetweenLengths(1, 4) },
                { "Between_Lengths_1_And_8_And_Elements_0_And_100", Gen.Int32().Between(0, 100).ListOf().BetweenLengths(1, 8) },
            };

            var biases = new[] { Gen.Bias.None, Gen.Bias.WithSize };

            foreach (var seed in seeds)
            foreach (var (betweenDescription, gen) in gens)
            foreach (var bias in biases)
            {
                var gen0 = gen.WithLengthBias(bias);

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
