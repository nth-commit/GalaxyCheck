using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Xunit.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutReplay
    {
        [Property(Skip = "Intentionally skipped for testing purposes")]
        private void PropertyWithNoReplay()
        {
        }

        [Property(Skip = "Intentionally skipped for testing purposes")]
        [Replay("Re-re-rewiiiind")]
        private void PropertyWithReplay()
        {
        }

        [Sample(Skip = "Intentionally skipped for testing purposes")]
        private void SampleWithNoReplay()
        {
        }

        [Sample(Skip = "Intentionally skipped for testing purposes")]
        [Replay("Re-re-rewiiiiiiiiind")]
        private void SampleWithReplay()
        {
        }

        [Theory]
        [InlineData(nameof(PropertyWithNoReplay), null)]
        [InlineData(nameof(PropertyWithReplay), "Re-re-rewiiiind")]
        [InlineData(nameof(SampleWithNoReplay), null)]
        [InlineData(nameof(SampleWithReplay), "Re-re-rewiiiiiiiiind")]
        public void ItPassesThroughReplay(string testMethodName, string? expectedReplay)
        {
            var testClassType = typeof(AboutReplay);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { });

            result.Parameters.Replay.Should().Be(expectedReplay);
        }

        private static MethodInfo GetMethod(string name)
        {
            var methodInfo = typeof(AboutReplay).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
