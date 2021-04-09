using FluentAssertions;
using GalaxyCheck;
using System;
using System.Linq;
using System.Reflection;
using Xunit;

namespace Tests.V2.PropertyTests.ReflectedPropertyTests
{
    public class AboutArity
    {
        private static int CalculateArity<T>(Test<T> test) => test.PresentedInput?.Length ?? 0;

        private void AVoidMethodWithNoArguments() { }

        [Fact]
        public void AVoidMethodWithNoArgumentsHasZeroArity()
        {
            var methodInfoProperty = Property.Reflect(GetMethod(nameof(AVoidMethodWithNoArguments)), this);

            var test = methodInfoProperty.SampleOne(seed: 0);

            CalculateArity(test).Should().Be(0);
        }

        private void AVoidMethodWithOneArgument(int x) { }

        [Fact]
        public void AVoidMethodWithOneArgumentHasOneArity()
        {
            var methodInfoProperty = Property.Reflect(GetMethod(nameof(AVoidMethodWithOneArgument)), this);

            var test = methodInfoProperty.SampleOne(seed: 0);

            CalculateArity(test).Should().Be(1);
        }

        private bool ABooleanMethodWithNoArguments() => true;

        [Fact]
        public void ABooleanMethodWithNoArgumentsHasZeroArity()
        {
            var methodInfoProperty = Property.Reflect(GetMethod(nameof(ABooleanMethodWithNoArguments)), this);

            var test = methodInfoProperty.SampleOne(seed: 0);

            CalculateArity(test).Should().Be(0);
        }

        private bool ABooleanMethodWithOneArgument(int x) => true;

        [Fact]
        public void ABooleanMethodWithOneArgumentHasOneArity()
        {
            var methodInfoProperty = Property.Reflect(GetMethod(nameof(ABooleanMethodWithOneArgument)), this);

            var test = methodInfoProperty.SampleOne(seed: 0);

            CalculateArity(test).Should().Be(1);
        }

        private Property AMethodWithOneArgumentReturningAPropertyWithOneArgument(int x) => Gen.Int32().ForAll(y => { });

        [Fact]
        public void AMethodWithOneArgumentReturningAPropertyWithOneArgumentHasTwoArity()
        {
            var methodInfoProperty = Property.Reflect(GetMethod(nameof(AMethodWithOneArgumentReturningAPropertyWithOneArgument)), this);

            var test = methodInfoProperty.SampleOne(seed: 0);

            CalculateArity(test).Should().Be(2);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutArity).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
