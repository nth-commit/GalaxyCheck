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
        public NebulaCheck.IGen<Test> ItErrorsWhenCountIsNegative() =>
            from elementGen in DomainGen.Gen()
            from count in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen).OfCount(count);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ListGen: 'count' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumCountIsNegative() =>
            from elementGen in DomainGen.Gen()
            from minCount in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen).WithCountGreaterThanEqual(minCount);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ListGen: 'minCount' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMaximumCountIsNegative() =>
            from elementGen in DomainGen.Gen()
            from maxCount in Gen.Int32().LessThan(0)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.List(elementGen).WithCountLessThanEqual(maxCount);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithMessage("Error while running generator ListGen: 'maxCount' cannot be negative");
            });

        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenMinimumCountIsGreaterThanMaximumCount() =>
            from elementGen in DomainGen.Gen()
            from minCount in Gen.Int32().Between(1, 20)
            from maxCount in Gen.Int32().GreaterThanEqual(0).LessThan(minCount)
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                void ShouldError(IListGen<object> gen)
                {
                    Action action = () => gen.SampleOne(seed: seed, size: size);

                    action.Should()
                        .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                        .WithMessage("Error while running generator ListGen: 'minCount' cannot be greater than 'maxCount'");
                }

                ShouldError(GalaxyCheck.Gen.List(elementGen).WithCountGreaterThanEqual(minCount).WithCountLessThanEqual(maxCount));
                ShouldError(GalaxyCheck.Gen.List(elementGen).WithCountLessThanEqual(maxCount).WithCountGreaterThanEqual(minCount));
            });
    }
}
