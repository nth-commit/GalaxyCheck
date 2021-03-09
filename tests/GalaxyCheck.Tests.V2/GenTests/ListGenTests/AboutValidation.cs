using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using NebulaCheck.Xunit;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using GalaxyCheck.Gens;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenLengthIsNegative() =>
            from elementGen in DomainGen.Gen()
            from length in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen).OfLength(length);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ListGen: 'length' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumLengthIsNegative() =>
            from elementGen in DomainGen.Gen()
            from minLength in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen).OfMinimumLength(minLength);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ListGen: 'minLength' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMaximumLengthIsNegative() =>
            from elementGen in DomainGen.Gen()
            from maxLength in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen).OfMaximumLength(maxLength);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ListGen: 'maxLength' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumLengthIsGreaterThanMaximumLength() =>
            from elementGen in DomainGen.Gen()
            from minLength in Gen.Int32().Between(1, 20)
            from maxLength in Gen.Int32().GreaterThanEqual(0).LessThan(minLength)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                void ShouldError(IListGen<object> gen)
                {
                    Action action = () => gen.SampleOne(seed: seed, size: size);

                    action.Should()
                        .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                        .WithMessage("Error while running generator ListGen: 'minLength' cannot be greater than 'maxLength'");
                }

                ShouldError(GalaxyCheck.Gen.List(elementGen).OfMinimumLength(minLength).OfMaximumLength(maxLength));
                ShouldError(GalaxyCheck.Gen.List(elementGen).OfMaximumLength(maxLength).OfMinimumLength(minLength));
            });
    }
}
