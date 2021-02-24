using FluentAssertions;
using System;
using System.Reflection;
using Xunit;

using GalaxyCheck;

namespace Tests.V2.PropertyTests.AboutMethodInfoProperties
{
    public class AboutMethodInfoOutput
    {
        private void AnInfallibleVoidPropertyFunction() { }

        [Fact]
        public void AVoidMethodInfoCanBeUnfalsifiable()
        {
            Property<object> property = GetMethod(nameof(AnInfallibleVoidPropertyFunction)).ToProperty(this);

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
            Property<object> property = GetMethod(nameof(AFallibleVoidPropertyFunction)).ToProperty(this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private bool AnInfallibleBooleanPropertyFunction() => true;

        [Fact]
        public void ABooleanMethodInfoCanBeUnfalsifiable()
        {
            Property<object> property = GetMethod(nameof(AnInfallibleBooleanPropertyFunction)).ToProperty(this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private bool AFallibleBooleanPropertyFunction(int x) => x < 100;

        [Fact]
        public void ABooleanMethodInfoCanBeFalsified()
        {
            Property<object> property = GetMethod(nameof(AFallibleBooleanPropertyFunction)).ToProperty(this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeTrue();
        }

        private Property AnInfallibleNestedPropertyFunction(int x) => Gen.Constant<object?>(null).ForAll(_ => true);

        [Fact]
        public void ANestedPropertyMethodInfoCanBeUnfalsifiable()
        {
            Property<object> property = GetMethod(nameof(AnInfallibleNestedPropertyFunction)).ToProperty(this);

            var checkResult = property.Check();

            checkResult.Falsified.Should().BeFalse();
        }

        private Property AFallibleNestedPropertyFunction(int x) => Gen.Int32().ForAll(x => x < 100);

        [Fact]
        public void ANestedPropertyMethodInfoCanBeFalsified()
        {
            Property<object> property = GetMethod(nameof(AFallibleNestedPropertyFunction)).ToProperty(this);

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
