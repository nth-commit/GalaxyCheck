using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;
using static Tests.V2.DomainGenAttributes;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.NullableGenTests
{
    public class AboutNullableGeneration
    {
        [Property]
        public void NullDoesNotShrink([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String());

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            Property.Precondition(sample.First() == null);

            sample.Should().HaveCount(1);
        }

        [Property]
        public void NonNullShrinksToNull([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String());

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            Property.Precondition(sample.First() != null);

            sample.Should().HaveElementAt(1, null);
        }

        [Fact]
        public void Snapshots()
        {
            var seeds = Enumerable.Range(0, 1);

            foreach (var seed in seeds)
            {
                var sample = GalaxyCheck.Gen.Nullable(GalaxyCheck.Gen.String()).Sample(seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
