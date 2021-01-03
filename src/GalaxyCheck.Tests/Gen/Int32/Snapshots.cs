using Xunit;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class Snapshots
    {
        [Fact]
        public void Example() => SnapshotGenValues(G.Int32());

        [Fact]
        public void ExampleOfExampleSpaces() => SnapshotGenExampleSpaces(G.Int32().Between(-10, 10));
    }
}
