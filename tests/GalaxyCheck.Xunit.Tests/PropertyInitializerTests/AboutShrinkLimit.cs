using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Xunit.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutShrinkLimit
    {
        [Property(Skip = "Intentionally skipped for testing purposes")]
        private void PropertyWithDefaultShrinkLimit()
        {
        }

        [Property(Skip = "Intentionally skipped for testing purposes", ShrinkLimit = 10)]
        private void PropertyWith10ShrinkLimit()
        {
        }

        [Sample(Skip = "Intentionally skipped for testing purposes")]
        private void SampleWithDefaultShrinkLimit()
        {
        }

        [Sample(Skip = "Intentionally skipped for testing purposes", ShrinkLimit = 10)]
        private void SampleWith10ShrinkLimit()
        {
        }

        [Theory]
        [InlineData(nameof(PropertyWithDefaultShrinkLimit), 500)]
        [InlineData(nameof(PropertyWith10ShrinkLimit), 10)]
        [InlineData(nameof(SampleWithDefaultShrinkLimit), 500)]
        [InlineData(nameof(SampleWith10ShrinkLimit), 10)]
        public void ItPassesThroughShrinkLimit(string testMethodName, int expectedShrinkLimit)
        {
            var testClassType = typeof(AboutShrinkLimit);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { });

            result.Parameters.ShrinkLimit.Should().Be(expectedShrinkLimit);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutShrinkLimit).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
