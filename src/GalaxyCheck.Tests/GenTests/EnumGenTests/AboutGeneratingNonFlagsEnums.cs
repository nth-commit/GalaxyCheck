using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.EnumGenTests
{
    public class AboutGeneratingNonFlagsEnums
    {
        private enum NgāTae
        {
            Mā = 0,

            Whero = 1,

            Kōwhai = 2
        }

        [Property]
        public void ItProducesValuesLikeElement([Seed] int seed, [Size] int size)
        {
            List<NgāTae> SampleTraversal(GalaxyCheck.IGen<NgāTae> gen) => AboutGeneratingNonFlagsEnums.SampleTraversal(gen, seed, size);

            var gen0 = GalaxyCheck.Gen.Enum<NgāTae>();
            var gen1 = GalaxyCheck.Gen.Element(new[] { NgāTae.Mā, NgāTae.Whero, NgāTae.Kōwhai });

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
