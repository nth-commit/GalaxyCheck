using FluentAssertions;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.BooleanGenTests
{
    public class AboutBooleanGeneration
    {
        private static IGen<List<bool>> Sample =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select GalaxyCheck.Gen.Boolean().SampleOneTraversal(seed: seed, size: size);

        private static IGen<List<bool>> FalseSample => Sample.Where(s => s.First() == false);


        [Property]
        public void FalseDoesNotShrink([MemberGen(nameof(FalseSample))] List<bool> sample)
        {
            sample.Should().HaveCount(1);
        }

        private static IGen<List<bool>> TrueSample => Sample.Where(s => s.First() == true);

        [Property]
        public void TrueShrinksToFalse([MemberGen(nameof(TrueSample))] List<bool> sample)
        {
            sample.Should().HaveCount(2);
            sample.Should().EndWith(false);
        }

        [Fact]
        public void Snapshots()
        {
            var seeds = Enumerable.Range(0, 1);

            foreach (var seed in seeds)
            {
                var sample = GalaxyCheck.Extensions.Sample(GalaxyCheck.Gen.Boolean(), seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
