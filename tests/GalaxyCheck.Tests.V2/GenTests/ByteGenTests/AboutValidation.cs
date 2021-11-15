using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;

namespace Tests.V2.GenTests.ByteGenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumIsGreaterThanMaximum() =>
            from min in Gen.Byte().GreaterThan(byte.MinValue)
            from max in Gen.Byte().LessThan(min)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Byte().GreaterThanEqual(min).LessThanEqual(max);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Byte: 'min' cannot be greater than 'max'");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumIsGreaterThanOrigin() =>
            from min in Gen.Byte().GreaterThan(byte.MinValue)
            from origin in Gen.Byte().LessThan(min)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Byte().GreaterThanEqual(min).ShrinkTowards(origin);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Byte: 'origin' must be between 'min' and 'max'");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMaximumIsLessThanOrigin() =>
            from max in Gen.Byte().LessThan(byte.MaxValue)
            from origin in Gen.Byte().GreaterThan(max)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Byte().LessThanEqual(max).ShrinkTowards(origin);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator Byte: 'origin' must be between 'min' and 'max'");
            });
    }
}
