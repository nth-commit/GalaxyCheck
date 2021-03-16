using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int32GenTests
{
    public class AboutShrinking
    {
        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheOrigin() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from origin in Gen.Int32()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().WithBias(bias).ShrinkTowards(origin);

                var minimum = gen.Minimum(seed: seed);

                minimum.Should().Be(origin);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheLocalMinimum_ForAPositiveRange() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from origin in Gen.Int32().Between(0, 100)
            from localMin in Gen.Int32().Between(origin, 200)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().WithBias(bias).ShrinkTowards(origin);

                var minimum = gen.Minimum(seed: seed, pred: value => value >= localMin);

                minimum.Should().Be(localMin);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheLocalMinimum_ForANegativeRange() =>
            from bias in DomainGen.Element(GalaxyCheck.Gen.Bias.None, GalaxyCheck.Gen.Bias.WithSize)
            from origin in Gen.Int32().Between(0, -100)
            from localMin in Gen.Int32().Between(origin, -200)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int32().WithBias(bias).ShrinkTowards(origin);

                var minimum = gen.Minimum(seed: seed, pred: value => value <= localMin);

                minimum.Should().Be(localMin);
            });
    }
}
