using FluentAssertions;
using System;
using System.Reflection;
using Xunit;

using GalaxyCheck;

namespace Tests.V2.PropertyTests.MethodPropertyTests
{
    public class AboutMethodInfoOutput
    {
        private void AnInfallibleVoidPropertyFunction() { }

        [Fact]
        public void AVoidMethodInfoCanBeUnfalsifiable()
        {
            Property<object> property = MethodProperty.Create(GetMethod(nameof(AnInfallibleVoidPropertyFunction)), this);

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
            Property<object> property = MethodProperty.Create(GetMethod(nameof(AFallibleVoidPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private bool AnInfallibleBooleanPropertyFunction() => true;

        [Fact]
        public void ABooleanMethodInfoCanBeUnfalsifiable()
        {
            Property<object> property = MethodProperty.Create(GetMethod(nameof(AnInfallibleBooleanPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private bool AFallibleBooleanPropertyFunction(int x) => x < 100;

        [Fact]
        public void ABooleanMethodInfoCanBeFalsified()
        {
            Property<object> property = MethodProperty.Create(GetMethod(nameof(AFallibleBooleanPropertyFunction)), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private Property AnInfallibleNestedPropertyFunction(int x) => Gen.Constant<object?>(null).ForAll(_ => true);
        private Property AnInfallibleNestedPropertyFunctionWithVaryingTypes(int x) => Gen.Constant(true).ForAll(_ => true);

        [Theory]
        [InlineData(nameof(AnInfallibleNestedPropertyFunction))]
        [InlineData(nameof(AnInfallibleNestedPropertyFunctionWithVaryingTypes))]
        public void ANestedPropertyMethodInfoCanBeUnfalsifiable(string methodName)
        {
            Property<object> property = MethodProperty.Create(GetMethod(methodName), this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private Property AFallibleNestedPropertyFunction(int x) => Gen.Int32().ForAll(x => x < 100);

        [Fact]
        public void ANestedPropertyMethodInfoCanBeFalsified()
        {
            Property<object> property = MethodProperty.Create(GetMethod(nameof(AFallibleNestedPropertyFunction)), this);

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
