using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Internal;
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
        [InlineData(nameof(Properties.PropertyWith10Iterations), 10)]
        [InlineData(nameof(Properties.SampleWith10Iterations), 10)]
        public void ItPassesThroughIterations(string testMethodName, int expectedIterations)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalPropertyConfiguration { DefaultIterations = 999 });

            result.Parameters.Iterations.Should().Be(expectedIterations);
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithDefaultIterations))]
        [InlineData(nameof(Properties.SampleWithDefaultIterations))]
        public void ItPassesThroughTheDefaultIterationsIfNotExplicitlySet(string testMethodName)
        {
            var defaultIterations = 1000;
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalPropertyConfiguration { DefaultIterations = defaultIterations });

            result.Parameters.Iterations.Should().Be(defaultIterations);
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
