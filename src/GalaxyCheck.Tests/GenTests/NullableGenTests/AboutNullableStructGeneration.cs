using FluentAssertions;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.NullableGenTests
{
    public class AboutNullableStructGeneration
    {
        private static readonly GalaxyCheck.IGen<int?> GenUnderTest = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32());

        private static IGen<List<int?>> Sample =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select GenUnderTest.SampleOneTraversal(seed: seed, size: size);

        private static IGen<List<int?>> NullableSample => Sample.Where(s => s.First() == null);

        [Property]
        public void NullDoesNotShrink([MemberGen(nameof(NullableSample))] List<int?> sample)
        {
            sample.Should().HaveCount(1);
        }

        private static IGen<List<int?>> NonNullableSample => Sample.Where(s => s.First() != null);

        [Property]
        public void NonNullShrinksToNull([MemberGen(nameof(NonNullableSample))] List<int?> sample)
        {
            sample.Should().HaveElementAt(1, null);
        }

        [Fact]
        public void Snapshots()
        {
            var seeds = Enumerable.Range(0, 1);

            foreach (var seed in seeds)
            {
                var sample = GalaxyCheck.Extensions.Sample(GenUnderTest, seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
