using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutDefaults
    {
        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMinimumLengthIsZero() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ImmutableList<object>> SampleTraversal(GalaxyCheck.IGen<ImmutableList<object>> gen) =>
                    AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = elementGen.ListOf();
                var gen1 = elementGen.ListOf().OfMinimumLength(0);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultMaximumLengthIsTwenty() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ImmutableList<object>> SampleTraversal(GalaxyCheck.IGen<ImmutableList<object>> gen) =>
                    AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = elementGen.ListOf();
                var gen1 = elementGen.ListOf().OfMaximumLength(20);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        [Property]
        public NebulaCheck.IGen<Test> TheDefaultLengthBiasIsWithSize() =>
            from elementGen in DomainGen.Gen()
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                List<ImmutableList<object>> SampleTraversal(GalaxyCheck.IGen<ImmutableList<object>> gen) =>
                    AboutDefaults.SampleTraversal(gen, seed: seed, size: size);

                var gen0 = elementGen.ListOf();
                var gen1 = elementGen.ListOf().WithLengthBias(GalaxyCheck.Gen.Bias.WithSize);

                SampleTraversal(gen0).Should().BeEquivalentTo(SampleTraversal(gen1));
            });

        private static List<ImmutableList<object>> SampleTraversal(GalaxyCheck.IGen<ImmutableList<object>> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();
    }
}
