using FluentAssertions;
using System;
using System.Reflection;
using Xunit;

using GalaxyCheck;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
{
    public class AboutMethodInfoOutput
    {
        private void AnInfallibleVoidPropertyFunction() { }

        [Fact]
        public void AVoidMethodInfoCanBeUnfalsifiable()
        {
            var property = Property.Reflect(GetMethod(nameof(AnInfallibleVoidPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private void AFallibleVoidPropertyFunction(int x)
        {
            Assert.True(x < 100);
        }

        [Fact]
        public void AVoidMethodInfoCanBeFalsified()
        {
            var property = Property.Reflect(GetMethod(nameof(AFallibleVoidPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private bool AnInfallibleBooleanPropertyFunction() => true;

        [Fact]
        public void ABooleanMethodInfoCanBeUnfalsifiable()
        {
            var property = Property.Reflect(GetMethod(nameof(AnInfallibleBooleanPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private bool AFallibleBooleanPropertyFunction(int x) => x < 100;

        [Fact]
        public void ABooleanMethodInfoCanBeFalsified()
        {
            var property = Property.Reflect(GetMethod(nameof(AFallibleBooleanPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private Property AnInfalliblePropertyFunction() => Gen.Constant<object?>(null).ForAll(_ => true);
        private Property AnInfalliblePropertyFunctionWithVaryingTypes() => Gen.Constant(true).ForAll(_ => true);

        [Theory]
        [InlineData(nameof(AnInfalliblePropertyFunction))]
        [InlineData(nameof(AnInfalliblePropertyFunctionWithVaryingTypes))]
        public void APropertyMethodInfoCanBeUnfalsifiable(string methodName)
        {
            var property = Property.Reflect(GetMethod(methodName), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private Property AFalliblePropertyFunction() => Gen.Int32().ForAll(x => x < 100);

        [Fact]
        public void APropertyMethodInfoCanBeFalsified()
        {
            var property = Property.Reflect(GetMethod(nameof(AFalliblePropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutMethodInfoOutput).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
