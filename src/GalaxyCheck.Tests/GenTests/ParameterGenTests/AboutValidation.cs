using FluentAssertions;
using GalaxyCheck;
using System;
using System.Reflection;
using Xunit;

namespace Tests.V2.GenTests.ParameterGenTests
{
    public class AboutValidation
    {
        private class StringGenAttribute : GenAttribute
        {
            public override IGen Value => Gen.String();
        }

        private static void PropertyWhereTypeOfGenIsNotAssignableToTypeOfParameter([StringGen] int x) { }

        /// <summary>
        /// ParametersGen uses ReflectedGen to generate individual parameters. We should just check that one error message
        /// that we know originates in ReflectedGen is routed through correctly. Then we can assume that all errors from
        /// ReflectedGen are handled in the same fashion.
        /// </summary>
        [Fact]
        public void ItErrorsWhenTheReflectedGenMightHaveErrored()
        {
            var gen = Gen.Parameters(GetMethod(nameof(PropertyWhereTypeOfGenIsNotAssignableToTypeOfParameter)));

            Action test = () => gen.SampleOne();

            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator ParametersGen: unable to generate value for parameter 'x', type 'System.String' was not assignable to the type it was registered to, 'System.Int32'");
        }

        private class AnotherStringGenAttribute : GenAttribute
        {
            public override IGen Value => Gen.String();
        }

        private static void PropertyWithMultipleGenAttributes([StringGen][AnotherStringGen] string x) { }

        [Fact]
        public void ItErrorsWhenMultipleGenAttributesAreProvided()
        {
            var gen = Gen.Parameters(GetMethod(nameof(PropertyWithMultipleGenAttributes)));

            Action test = () => gen.SampleOne();

            test.Should()
                .Throw<Exceptions.GenErrorException>()
                .WithMessage("Error while running generator ParametersGen: unable to generate value for parameter 'x', multiple GenAttributes is unsupported");
        }

        private static MethodInfo GetMethod(string name)
        {
            return typeof(AboutValidation).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static)!;
        }
    }
}
