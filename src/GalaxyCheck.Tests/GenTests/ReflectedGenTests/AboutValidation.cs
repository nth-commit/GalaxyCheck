using FluentAssertions;
using GalaxyCheck;
using System;
using Xunit;

namespace Tests.V2.GenTests.ReflectedGenTests
{
    public class AboutValidation
    {
        [Fact]
        public void ItErrorsWhenTypeOfRegisteredGeneratorIsNotAssignableToGivenType()
        {
            var gen = Gen
                .Factory()
                .RegisterType(typeof(string), Gen.Int32())
                .Create<string>();

            Action test = () => gen.SampleOne();

            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithGenErrorMessage("type 'System.Int32' was not assignable to the type it was registered to, 'System.String'");
        }
    }
}
