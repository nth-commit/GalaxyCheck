using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Linq;
using Xunit;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.EnumGenTests
{
    public class AboutGeneratingFlagsEnums
    {
        [Flags]
        private enum AllowedOperations
        {
            Create = 1 << 0,

            Read = 1 << 1,

            Update = 1 << 2,

            Delete = 1 << 3
        }

        [Property]
        public void ItShrinksTowardsTheSmallestValue([Seed] int seed)
        {
            var gen = GalaxyCheck.Gen.Enum<AllowedOperations>();

            var minimum = gen.Minimum(seed: seed);

            minimum.Should().Be(AllowedOperations.Create);
        }

        [Fact]
        public void Snapshots()
        {
            // Should demonstrate the combinatorial factor when generating a flags enum

            var seeds = Enumerable.Range(0, 3);

            var gen = GalaxyCheck.Gen.Enum<AllowedOperations>();

            foreach (var seed in seeds)
            {
                var sample = gen
                    .Advanced.SampleOneExampleSpace(seed: seed, size: 75)
                    .Take(500)
                    .Render(x => x.ToString());

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
