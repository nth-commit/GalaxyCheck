using FluentAssertions;
using GalaxyCheck;
using System;
using Xunit;

namespace Tests.V2.GenTests.AutoGenTests
{
    public class AboutValidation
    {
        [Fact]
        public void ItErrorsWhenTypeOfRegisteredGeneratorIsNotAssignableToGivenType()
        {
            var gen = Gen
                .AutoFactory()
                .RegisterType(typeof(string), Gen.Int32())
                .Create<string>();

            Action test = () => gen.SampleOne();

            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator AutoGen: type 'System.Int32' was not assignable to the type it was registered to, 'System.String'");
        }
    }
}
