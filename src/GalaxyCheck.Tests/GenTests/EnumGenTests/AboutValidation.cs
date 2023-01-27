using FluentAssertions;
using GalaxyCheck;
using NebulaCheck;
using System;
using static Tests.V2.DomainGenAttributes;

namespace Tests.V2.GenTests.EnumGenTests
{
    public class AboutValidation
    {
        [Property]
        public void ItErrorsWhenTypeIsNotAnEnum_Int32([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Enum<int>();

            Action action = () => gen.SampleOne(seed: seed, size: size);

            action.Should()
                .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                .WithGenErrorMessage("Type 'System.Int32' is not an enum");
        }

        [Property]
        public void ItErrorsWhenTypeIsNotAnEnum_Decimal([Seed] int seed, [Size] int size)
        {
            var gen = GalaxyCheck.Gen.Enum<decimal>();

            Action action = () => gen.SampleOne(seed: seed, size: size);

            action.Should()
                .Throw<GalaxyCheck.Exceptions.GenErrorException>()
                .WithGenErrorMessage("Type 'System.Decimal' is not an enum");
        }
    }
}
