using Xunit;
using GC = GalaxyCheck;
using GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    public class Snapshots
    {
        [Fact]
        public void Snapshot_Between_0_And_1000()
        {
            TestWithSize(size =>
            {
                SnapshotGenValues(gen: GC.Gen.Int32().Between(0, 1000), repeatSeed: 0, size: size);
            });
        }

        [Fact]
        public void Snapshot_Between_Negative_10_And_10()
        {
            TestWithSize(size =>
            {
                SnapshotGenValues(gen: GC.Gen.Int32().Between(-10, 10), repeatSeed: 0, size: size);
            });
        }

        [Fact]
        public void Snapshot_ExampleSpaces_Between_Negative_10_And_10() =>
            SnapshotGenExampleSpaces(GC.Gen.Int32().Between(-10, 10));

        [Fact]
        public void Snapshot_ExampleSpaces_Between_0_And_10_With_Origin_5() =>
            SnapshotGenExampleSpaces(GC.Gen.Int32().Between(0, 10).ShrinkTowards(5));

        [Fact]
        public void Snapshot_ExampleSpaces_Between_Negative_10_And_0_With_Origin_Negative_5() =>
            SnapshotGenExampleSpaces(GC.Gen.Int32().Between(-10, 0).ShrinkTowards(-5));
    }
}
