using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutSize
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class Properties
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property]
            public void PropertyWithDefaultSize()
            {
            }

            [Property(Size = 0)]
            public void PropertyWithSize0()
            {
            }

            [Property(Size = 10)]
            public void PropertyWithSize10()
            {
            }

            [Property(Size = 10)]
            [Replay("===asdlasd;lask;lzk;lk")]
            public void PropertyWithReplayAndSize()
            {
            }

            [Sample]
            public void SampleWithDefaultSize()
            {
            }

            [Sample(Size = 0)]
            public void SampleWithSize0()
            {
            }

            [Sample(Size = 10)]
            public void SampleWithSize10()
            {
            }

            [Sample(Size = 10)]
            [Replay("===asdlasd;lask;lzk;lk")]
            public void SampleWithReplayAndSize()
            {
            }
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithSize0), 0)]
        [InlineData(nameof(Properties.PropertyWithSize10), 10)]
        [InlineData(nameof(Properties.SampleWithSize0), 0)]
        [InlineData(nameof(Properties.SampleWithSize10), 10)]
        public void ItPassesThroughSize(string testMethodName, int expectedSize)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalConfiguration());

            result.Parameters.Size.Should().Be(expectedSize);
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithDefaultSize))]
        [InlineData(nameof(Properties.PropertyWithReplayAndSize))]
        [InlineData(nameof(Properties.SampleWithDefaultSize))]
        [InlineData(nameof(Properties.SampleWithReplayAndSize))]
        public void ItDoesNotPassThroughSize(string testMethodName)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(
                testClassType,
                testMethodInfo,
                new object[] { },
                new DefaultPropertyFactory(),
                new GlobalConfiguration());

            result.Parameters.Size.Should().Be(null);
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
