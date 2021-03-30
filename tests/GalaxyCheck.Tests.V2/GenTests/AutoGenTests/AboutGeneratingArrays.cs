using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutGeneratingArrays
    {
        [Property]
        public NebulaCheck.IGen<Test> ItCanGenerateArrays() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<int[]> SampleTraversal(GalaxyCheck.IGen<int[]> gen) =>
                    AboutGeneratingArrays.SampleTraversal(gen, seed, size);

                var gen0 = GalaxyCheck.Gen.Auto<int[]>();
                var gen1 = GalaxyCheck.Gen.Int32().ListOf().Select(x => x.ToArray());

                var sample0 = SampleTraversal(gen0);
                var sample1 = SampleTraversal(gen1);

                sample0.Should().BeEquivalentTo(sample1);
            });

        private static List<T> SampleTraversal<T>(GalaxyCheck.IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
