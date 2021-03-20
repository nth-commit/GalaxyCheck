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
        [Property(Skip = "Intentionally skipped for testing purposes")]
        private void PropertyWithDefaultIterations()
        {
        }

        [Property(Skip = "Intentionally skipped for testing purposes", Iterations = 10)]
        private void PropertyWith10Iterations()
        {
        }

        [Sample(Skip = "Intentionally skipped for testing purposes")]
        private void SampleWithDefaultIterations()
        {
        }

        [Sample(Skip = "Intentionally skipped for testing purposes", Iterations = 10)]
        private void SampleWith10Iterations()
        {
        }

        [Theory]
        [InlineData(nameof(PropertyWithDefaultIterations), 100)]
        [InlineData(nameof(PropertyWith10Iterations), 10)]
        [InlineData(nameof(SampleWithDefaultIterations), 100)]
        [InlineData(nameof(SampleWith10Iterations), 10)]
        public void ItPassesThroughIterations(string testMethodName, int expectedIterations)
        {
            var testClassType = typeof(AboutIterations);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { });

            result.Parameters.Iterations.Should().Be(expectedIterations);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutIterations).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
