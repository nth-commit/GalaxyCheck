using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using System.Linq;
using Test = NebulaCheck.Test;
using Property = NebulaCheck.Property;
using Gen = NebulaCheck.Gen;

namespace Tests.V2.GenTests.CharGenTests
{
    public class AboutValidation
    {
        [Property]
        public NebulaCheck.IGen<Test> ItErrorsWhenCharTypesIsNone() =>
            from charType in Gen.Choose(
                Gen.Int32().LessThanEqual(0).Select(x => (GalaxyCheck.Gen.CharType)x),
                Gen.Int32().GreaterThan((int)GalaxyCheck.Gen.CharType.All).Select(x => (GalaxyCheck.Gen.CharType)x))
            from seed in DomainGen.Seed()
            from size in DomainGen.Size()
            select Property.ForThese(() =>
            {
                var gen = GalaxyCheck.Gen.Char(charType);

                Action action = () => gen.SampleOne(seed: seed, size: size);

                action.Should()
                    .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                    .WithGenErrorMessage("'charType' was not a valid flag value");
            });
    }
}
