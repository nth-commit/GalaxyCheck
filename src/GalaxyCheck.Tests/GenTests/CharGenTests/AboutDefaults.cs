using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.CharGenTests
{
    public class AboutDefaults
    {
        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMinimumIsTheMinimumInt32() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen0 = GalaxyCheck.Gen.Char();
                var gen1 = GalaxyCheck.Gen.Char(GalaxyCheck.Gen.CharType.All);

                SampleTraversal(gen0, seed, size).Should().BeEquivalentTo(SampleTraversal(gen1, seed, size));
            });

        private static List<char> SampleTraversal(GalaxyCheck.IGen<char> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
