using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutReplay
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class Properties
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property]
            public void PropertyWithNoReplay()
            {
            }

            [Property]
            [Replay("Re-re-rewiiiind")]
            public void PropertyWithReplay()
            {
            }

            [Sample]
            public void SampleWithNoReplay()
            {
            }

            [Sample]
            [Replay("Re-re-rewiiiiiiiiind")]
            public void SampleWithReplay()
            {
            }
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithNoReplay), null)]
        [InlineData(nameof(Properties.PropertyWithReplay), "Re-re-rewiiiind")]
        [InlineData(nameof(Properties.SampleWithNoReplay), null)]
        [InlineData(nameof(Properties.SampleWithReplay), "Re-re-rewiiiiiiiiind")]
        public void ItPassesThroughReplay(string testMethodName, string? expectedReplay)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalPropertyConfiguration());

            result.Parameters.Replay.Should().Be(expectedReplay);
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
