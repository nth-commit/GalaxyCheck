using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutShrinkLimit
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class Properties
#pragma warning restore xUnit1000 // Test classes must be public
        { 
            [Property]
            public void PropertyWithDefaultShrinkLimit()
            {
            }

            [Property(ShrinkLimit = 10)]
            public void PropertyWith10ShrinkLimit()
            {
            }

            [Sample]
            public void SampleWithDefaultShrinkLimit()
            {
            }

            [Sample(ShrinkLimit = 10)]
            public void SampleWith10ShrinkLimit()
            {
            }
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWith10ShrinkLimit), 10)]
        [InlineData(nameof(Properties.SampleWith10ShrinkLimit), 10)]
        public void ItPassesThroughShrinkLimit(string testMethodName, int expectedShrinkLimit)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalPropertyConfiguration() { DefaultShrinkLimit = 999 });

            result.Parameters.ShrinkLimit.Should().Be(expectedShrinkLimit);
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithDefaultShrinkLimit))]
        [InlineData(nameof(Properties.SampleWithDefaultShrinkLimit))]
        public void ItPassesThroughTheDefaultShrinkLimitIfNotExplicitlySet(string testMethodName)
        {
            var defaultShrinkLimit = 1000;
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalPropertyConfiguration { DefaultShrinkLimit = defaultShrinkLimit });

            result.Parameters.ShrinkLimit.Should().Be(defaultShrinkLimit);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(Properties).GetMethod(name, BindingFlags.Instance | BindingFlags.Public);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
