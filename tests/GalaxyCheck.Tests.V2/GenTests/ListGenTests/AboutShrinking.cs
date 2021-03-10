using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutShrinking
    {
        [Property]
        public NebulaCheck.IGen<Test> ItShrinksTheLengthToTheMinimumLengthWhilstShrinkingTheElementsToTheirMinimums() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from elementGen in DomainGen.Gen()
            from minLength in Gen.Int32().Between(0, 20)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var elementMinimum = elementGen.Minimal(seed: seed);
                var gen = GalaxyCheck.Gen.ListOf(elementGen).OfMinimumLength(minLength);

                var minimum = gen.Minimal(seed: seed);

                minimum.Should()
                    .HaveCount(minLength).And
                    .AllBeEquivalentTo(elementMinimum);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksTheLengthToTheLocalMinimumLength() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from elementGen in DomainGen.Gen()
            from localMinLength in Gen.Int32().Between(0, 5)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.ListOf(elementGen);

                var minimum = gen.Minimal(seed: seed, pred: list => list.Count >= localMinLength);

                minimum.Should().HaveCount(localMinLength);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheSinglePredicatedElement() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from elementMinimum in Gen.Int32().Between(0, 100)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().Between(0, 100).ListOf();

                var minimum = gen.Minimal(seed: seed, pred: list => list.Any(element => element >= elementMinimum));

                minimum.Should().BeEquivalentTo(elementMinimum);
            });
    }
}
