using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.Int64GenTests
{
    public class AboutShrinking
    {
        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheOrigin() =>
            from bias in DomainGen.Bias()
            from origin in Gen.Int64()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().WithBias(bias).ShrinkTowards(origin);

                var minimum = gen.Minimum(seed: seed);

                minimum.Should().Be(origin);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheLocalMinimum_ForAPositiveRange() =>
            from bias in DomainGen.Bias()
            from origin in Gen.Int64().Between(0, 100)
            from localMin in Gen.Int64().Between(origin, 200)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().WithBias(bias).ShrinkTowards(origin);

                var minimum = gen.Minimum(seed: seed, pred: value => value >= localMin);

                minimum.Should().Be(localMin);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksToTheLocalMinimum_ForANegativeRange() =>
            from bias in DomainGen.Bias()
            from origin in Gen.Int64().Between(0, -100)
            from localMin in Gen.Int64().Between(origin, -200)
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().WithBias(bias).ShrinkTowards(origin);

                var minimum = gen.Minimum(seed: seed, pred: value => value <= localMin);

                minimum.Should().Be(localMin);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksAnyScenarioWithin128Attempts_ForAPositiveRange() =>
            from bias in DomainGen.Bias().NoShrink()
            from localMin in Gen.Int64().Between(0, long.MaxValue / 2).NoShrink()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().GreaterThanEqual(0).WithBias(bias);

                var minimum = gen.Advanced.MinimumWithMetrics(
                    seed: seed,
                    deepMinimum: false,
                    pred: value => value >= localMin);

                minimum.Shrinks.Should().BeLessOrEqualTo(128);
            });

        [Property]
        public NebulaCheck.IGen<Test> ItShrinksAnyScenarioWithin128Attempts_ForANegativeRange() =>
            from bias in DomainGen.Bias().NoShrink()
            from localMin in Gen.Int64().Between(0, long.MinValue / 2).NoShrink()
            from seed in DomainGen.Seed()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int64().LessThanEqual(0).WithBias(bias);

                var minimum = gen.Advanced.MinimumWithMetrics(
                    seed: seed,
                    deepMinimum: false,
                    pred: value => value <= localMin);

                minimum.Shrinks.Should().BeLessOrEqualTo(128);
            });
    }
}
