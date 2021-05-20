using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;
using static Tests.V2.DomainGenAttributes;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.BooleanGenTests
{
    public class AboutBooleanGeneration
    {
        [Property]
        public void FalseDoesNotShrink([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Boolean();

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            Property.Precondition(sample.First() == false);

            sample.Should().HaveCount(1);
        }

        [Property]
        public void TrueShrinksToFalse([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Boolean();

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            Property.Precondition(sample.First() == true);

            sample.Should().HaveCount(2);
            sample.Should().EndWith(false);
        }

        [Fact]
        public void Snapshots()
        {
            var seeds = Enumerable.Range(0, 1);

            foreach (var seed in seeds)
            {
                var sample = GalaxyCheck.Gen.Boolean().Sample(seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                        $"Seed_{seed}"
                    });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
