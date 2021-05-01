using FluentAssertions;
using GalaxyCheck;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutValidation
    {
        private class StringGenAttribute : GenProviderAttribute
        {
            public override IGen Get => Gen.String();
        }

        private static void PropertyWhereTypeOfGenIsNotAssignableToTypeOfParameter([StringGen] int x) { }

        [Fact]
        public void ItErrorsWhenTypeOfGenIsNotAssignableToTypeOfParameter()
        {
            var gen = Gen
                .Parameters(GetMethod(nameof(PropertyWhereTypeOfGenIsNotAssignableToTypeOfParameter)))
                .Select(x => x.Single())
                .Cast<int>();

            Action test = () => gen.SampleOne();

            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator ParametersGen: unable to generate value for parameter 'x', 'System.String' is not assignable to 'System.Int32'");
        }

        private class AnotherStringGenAttribute : GenProviderAttribute
        {
            public override IGen Get => Gen.String();
        }

        private static void PropertyWithMultipleGenAttributes([StringGen][AnotherStringGen] string x) { }

        [Fact]
        public void ItErrorsWhenMultipleGenAttributesAreProvided()
        {
            var gen = Gen
                .Parameters(GetMethod(nameof(PropertyWithMultipleGenAttributes)))
                .Select(x => x.Single())
                .Cast<string>();

            Action test = () => gen.SampleOne();

            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator ParametersGen: parameter 'x' has multiple GenProviderAttributes (unsupported)");
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(AboutValidation).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
        }
    }
}
