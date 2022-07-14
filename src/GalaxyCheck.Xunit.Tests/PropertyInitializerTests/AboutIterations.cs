using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Xunit.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutIterations
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class Properties
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property]
            public void PropertyWithDefaultIterations()
            {
            }

            [Property(Iterations = 10)]
            public void PropertyWith10Iterations()
            {
            }

            [Sample]
            public void SampleWithDefaultIterations()
            {
            }

            [Sample(Iterations = 10)]
            public void SampleWith10Iterations()
            {
            }
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithDefaultIterations), 100)]
        [InlineData(nameof(Properties.PropertyWith10Iterations), 10)]
        [InlineData(nameof(Properties.SampleWithDefaultIterations), 100)]
        [InlineData(nameof(Properties.SampleWith10Iterations), 10)]
        public void ItPassesThroughIterations(string testMethodName, int expectedIterations)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new PropertyFactorySync());

            result.Parameters.Iterations.Should().Be(expectedIterations);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(Properties).GetMethod(name, BindingFlags.Public | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
