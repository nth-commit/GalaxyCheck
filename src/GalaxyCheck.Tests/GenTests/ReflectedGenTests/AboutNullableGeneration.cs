using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutNullableGeneration
    {
        [Property]
        public void ItGeneratesNullableStructs([Seed] int seed, [Size] int size)
        {
            List<int?> SampleTraversal(GalaxyCheck.IGen<int?> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32());
            var gen1 = GalaxyCheck.Gen.Create<int?>();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        [Property]
        public void ItGeneratesNullablesStructsInNestedTypes([Seed] int seed, [Size] int size)
        {
            // Can't use a record with properties here now because the seed the property is generated with is hard to reproduce.

            List<IReadOnlyList<int?>> SampleTraversal(GalaxyCheck.IGen<IReadOnlyList<int?>> gen) =>
                AboutNullableGeneration.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.NullableStruct(GalaxyCheck.Gen.Int32()).ListOf();
            var gen1 = GalaxyCheck.Gen.Create<IReadOnlyList<int?>>();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
