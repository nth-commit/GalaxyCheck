using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutShrinking
    {
        [Property]
        public NebulaCheck.IGen<Test> ItShrinksTheCountToTheMinimumCountWhilstShrinkingTheElementsToTheirMinimums() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from elementGen in DomainGen.Gen()
            from minCount in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var elementMinimum = elementGen.Minimum(seed: seed);
                var gen = GalaxyCheck.Gen.List(elementGen).WithCountGreaterThanEqual(minCount);

                var minimum = gen.Minimum(seed: seed);

                minimum.Should()
                    .HaveCount(minCount).And
                    .AllBeEquivalentTo(elementMinimum);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksTheCountToTheLocalMinimumCount() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from elementGen in DomainGen.Gen()
            from localMinCount in Gen.Int32().Between(0, 5)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen);

                var minimum = gen.Minimum(seed: seed, pred: list => list.Count >= localMinCount);

                minimum.Should().HaveCount(localMinCount);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheSinglePredicatedElement() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from elementMinimum in Gen.Int32().Between(0, 100)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().Between(0, 100).ListOf();

                var minimum = gen.Minimum(seed: seed, pred: list => list.Any(element => element >= elementMinimum));

                minimum.Should().BeEquivalentTo(elementMinimum);
            });
    }
}
