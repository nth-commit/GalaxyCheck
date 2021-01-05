using Xunit;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    public class Snapshots
    {
        [Fact]
        public void Example() => SnapshotGenValues(GC.Gen.Int32());

        [Fact]
        public void ExampleOfExampleSpaces() => SnapshotGenExampleSpaces(GC.Gen.Int32().Between(-10, 10));

        [Fact]
        public void ExampleOfExampleSpaces_OriginVariant1() => SnapshotGenExampleSpaces(GC.Gen.Int32().Between(0, 10).ShrinkTowards(5));

        [Fact]
        public void ExampleOfExampleSpaces_OriginVariant2() => SnapshotGenExampleSpaces(GC.Gen.Int32().Between(-10, 0).ShrinkTowards(-5));
    }
}
