using FluentAssertions;
using GalaxyCheck;
using Xunit;

namespace Tests.GenSnapshotInitializerTests
{
    public class AboutSize
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshots
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [GenSnapshot]
            public object GenSnapshotWithDefaultSize() => new { };

            [GenSnapshot(Size = 0)]
            public object GenSnapshotWithSize0() => new { };

            [GenSnapshot(Size = 10)]
            public object GenSnapshotWithSize10() => new { };
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithSize0), 0)]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithSize10), 10)]
        public void ItPassesThroughSize(string testMethodName, int expectedSize)
        {
            var result = TestProxy.Initialize(typeof(GenSnapshots), testMethodName);

            result.Size.Should().Be(expectedSize);
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithDefaultSize))]
        public void ItPassesThroughTheDefaultSizeIfNotExplicitlySet(string testMethodName)
        {
            var result = TestProxy.Initialize(typeof(GenSnapshots), testMethodName, configure: config => config.DefaultSize = 66);

            result.Size.Should().Be(66);
        }
    }
}
