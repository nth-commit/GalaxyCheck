using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Collections.Generic;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutGeneratingEnums
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
        public void ItGeneratesFlagsEnumsLikeTheEnumGen([Seed] int seed, [Size] int size)
        {
            List<AllowedOperations> SampleTraversal(GalaxyCheck.IGen<AllowedOperations> gen) =>
                AboutGeneratingEnums.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Enum<AllowedOperations>();
            var gen1 = GalaxyCheck.Gen.Create<AllowedOperations>();

            var sample0 = SampleTraversal(gen0);
            var sample1 = SampleTraversal(gen1);

            sample0.Should().BeEquivalentTo(sample1);
        }

        private enum NgāTae
        {
            Mā = 0,

            Whero = 1,

            Kōwhai = 2
        }

        [Property]
        public void ItGeneratesNonFlagsEnumsLikeTheEnumGen([Seed] int seed, [Size] int size)
        {
            List<NgāTae> SampleTraversal(GalaxyCheck.IGen<NgāTae> gen) =>
                AboutGeneratingEnums.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Enum<NgāTae>();
            var gen1 = GalaxyCheck.Gen.Create<NgāTae>();

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
