using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Collections.Generic;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.StringGenTests
{
    public class AboutDefaults
    {
        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMinimumLengthIsZero() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.String();
                var gen1 = gen0.WithLengthGreaterThanEqual(0);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMaximumLengthIsOneHundred() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.String();
                var gen1 = gen0.WithLengthLessThanEqual(100);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultLengthBiasIsWithSize() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<string> SampleTraversal(GalaxyCheck.IGen<string> gen) =>
                    AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = GalaxyCheck.Gen.String();
                var gen1 = gen0.WithLengthBias(GalaxyCheck.Gen.Bias.WithSize);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        private static List<string> SampleTraversal(GalaxyCheck.IGen<string> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
