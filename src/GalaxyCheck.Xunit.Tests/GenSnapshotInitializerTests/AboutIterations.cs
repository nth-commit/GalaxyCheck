using FluentAssertions;
using GalaxyCheck;
using Xunit;

namespace Tests.GenSnapshotInitializerTests
{
    public class AboutIterations
    {
#pragma warning disable xUnit1000 // Test classes must be public
        private class GenSnapshots
#pragma warning restore xUnit1000 // Test classes must be public
        {
            [GenSnapshot]
            public object GenSnapshotWithDefaultIterations() => new { };

            [GenSnapshot(Iterations = 5)]
            public object GenSnapshotWith5Iterations() => new { };

            [GenSnapshot(Iterations = 10)]
            public object GenSnapshotWith10Iterations() => new { };
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWith5Iterations), new int[] { 0, 1, 2, 3, 4 })]
        [InlineData(nameof(GenSnapshots.GenSnapshotWith10Iterations), new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 })]
        public void ItPassesThroughSeedsForTheIterations(string testMethodName, int[] expectedSeeds)
        {
            var result = TestProxy.Initialize(typeof(GenSnapshots), testMethodName, configure: config => config.DefaultIterations = 999);

            result.Seeds.Should().BeEquivalentTo(expectedSeeds);
        }

        [Theory]
        [InlineData(nameof(GenSnapshots.GenSnapshotWithDefaultIterations))]
        public void ItPassesThroughSeedsFromTheDefaultIterationsIfNotExplicitlySet(string testMethodName)
        {
            var result = TestProxy.Initialize(typeof(GenSnapshots), testMethodName, configure: config => config.DefaultIterations = 5);

            result.Seeds.Should().BeEquivalentTo(new [] { 0, 1, 2, 3, 4 });
        }
    }
}
