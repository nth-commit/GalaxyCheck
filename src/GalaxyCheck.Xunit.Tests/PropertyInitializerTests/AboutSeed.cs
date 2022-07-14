using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.Xunit.Internal;
using System;
using System.Reflection;
using Xunit;

namespace Tests.PropertyInitializerTests
{
    public class AboutSeed
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class Properties
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [Property]
            public void PropertyWithDefaultSeed()
            {
            }

            [Property(Seed = 0)]
            public void PropertyWithSeed0()
            {
            }

            [Property(Seed = 10)]
            public void PropertyWithSeed10()
            {
            }

            [Property(Seed = 10)]
            [Replay("===asdlasd;lask;lzk;lk")]
            public void PropertyWithReplayAndSeed()
            {
            }

            [Sample]
            public void SampleWithDefaultSeed()
            {
            }

            [Sample(Seed = 0)]
            public void SampleWithSeed0()
            {
            }

            [Sample(Seed = 10)]
            public void SampleWithSeed10()
            {
            }

            [Sample(Seed = 10)]
            [Replay("===asdlasd;lask;lzk;lk")]
            public void SampleWithReplayAndSeed()
            {
            }
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithSeed0), 0)]
        [InlineData(nameof(Properties.PropertyWithSeed10), 10)]
        [InlineData(nameof(Properties.SampleWithSeed0), 0)]
        [InlineData(nameof(Properties.SampleWithSeed10), 10)]
        public void ItPassesThroughSeed(string testMethodName, int expectedSeed)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new PropertyFactorySync());

            result.Parameters.Seed.Should().Be(expectedSeed);
        }

        [Theory]
        [InlineData(nameof(Properties.PropertyWithDefaultSeed))]
        [InlineData(nameof(Properties.PropertyWithReplayAndSeed))]
        [InlineData(nameof(Properties.SampleWithDefaultSeed))]
        [InlineData(nameof(Properties.SampleWithReplayAndSeed))]
        public void ItDoesNotPassThroughSeed(string testMethodName)
        {
            var testClassType = typeof(Properties);
            var testMethodInfo = GetMethod(testMethodName);

            var result = PropertyInitializer.Initialize(testClassType, testMethodInfo, new object[] { }, new PropertyFactorySync());

            result.Parameters.Seed.Should().Be(null);
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
