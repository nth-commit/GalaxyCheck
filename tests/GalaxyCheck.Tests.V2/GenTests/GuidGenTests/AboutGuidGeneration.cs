using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.GuidGenTests
{
    public class AboutGuidGeneration
    {
        [Property]
        public void ItDoesNotShrink([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Guid();

            var sample = gen.SampleOneTraversal(seed: seed, size: size);

            sample.Should().HaveCount(1);
        }

        [Property]
        public void ItIsNotBiased([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Guid();

            var sample = gen.Sample(seed: seed, size: size);

            sample.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void Snapshots()
        {
            var seeds = Enumerable.Range(0, 3);

            foreach (var seed in seeds)
            {
                var sample = GalaxyCheck.Gen.Guid().Sample(seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
