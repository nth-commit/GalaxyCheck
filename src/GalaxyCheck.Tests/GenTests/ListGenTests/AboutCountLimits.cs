using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Gen = NebulaCheck.Gen;
using Property = NebulaCheck.Property;
using Test = NebulaCheck.Test;

namespace Tests.V2.GenTests.ListGenTests
{
    public class AboutCountLimits
    {
        [Property]
        public NebulaCheck.IGen<Test> ItHasACountLimitEnabledByDefault() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from elementGen in DomainGen.Gen()
            from excessiveCount in Gen.Int32().Between(1001, 2000)
            select Property.ForThese(() =>
            {
                void AssertCountLimit(GalaxyCheck.Gens.IListGen<object> gen)
                {
                    Action test = () => gen.SampleOne(seed: seed, size: size);

                    test.Should()
                        .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                        .WithGenErrorMessage(
                            "Count limit exceeded. " +
                            "This is a built-in safety mechanism to prevent hanging tests. " +
                            "If generating a list with over 1000 elements was intended, relax this constraint by calling IListGen.DisableCountLimitUnsafe().");
                }

                AssertCountLimit(elementGen.ListOf().WithCountGreaterThanEqual(excessiveCount));
                AssertCountLimit(elementGen.ListOf().WithCountLessThanEqual(excessiveCount));
                AssertCountLimit(elementGen.ListOf().WithCountBetween(excessiveCount, excessiveCount));
                AssertCountLimit(elementGen.ListOf().WithCount(excessiveCount));
            });

        [Property(Iterations = 10)]
        public NebulaCheck.IGen<Test> TheCountLimitCanBeDisabled() =>
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            from elementGen in DomainGen.Gen()
            from excessiveCount in Gen.Int32().Between(1001, 2000)
            select Property.ForThese(() =>
            {
                void AssertCountLimitDisabled(GalaxyCheck.Gens.IListGen<object> gen)
                {
                    gen.DisableCountLimitUnsafe().SampleOne(seed: seed, size: size);
                }

                AssertCountLimitDisabled(elementGen.ListOf().WithCountGreaterThanEqual(excessiveCount));
                AssertCountLimitDisabled(elementGen.ListOf().WithCountLessThanEqual(excessiveCount));
                AssertCountLimitDisabled(elementGen.ListOf().WithCountBetween(excessiveCount, excessiveCount));
                AssertCountLimitDisabled(elementGen.ListOf().WithCount(excessiveCount));
            });
    }
}
