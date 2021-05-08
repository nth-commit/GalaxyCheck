using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.Int16GenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumIsGreaterThanMaximum() =>
            from min in Gen.Int16().GreaterThan(short.MinValue)
            from max in Gen.Int16().LessThan(min)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int16().GreaterThanEqual(min).LessThanEqual(max);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Int16Gen: 'min' cannot be greater than 'max'");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumIsGreaterThanOrigin() =>
            from min in Gen.Int16().GreaterThan(short.MinValue)
            from origin in Gen.Int16().LessThan(min)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int16().GreaterThanEqual(min).ShrinkTowards(origin);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Int16Gen: 'origin' must be between 'min' and 'max'");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMaximumIsLessThanOrigin() =>
            from max in Gen.Int16().LessThan(short.MaxValue)
            from origin in Gen.Int16().GreaterThan(max)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Int16().LessThanEqual(max).ShrinkTowards(origin);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Int16Gen: 'origin' must be between 'min' and 'max'");
            });
    }
}
