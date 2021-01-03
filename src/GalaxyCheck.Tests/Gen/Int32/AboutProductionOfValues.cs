using Xunit;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutProductionOfValues
    {
        [Fact]
        public void Example() => SnapshotGenValues(G.Int32());

        [Fact]
        // TODO: Constrain these integers to a reasonable level to witness the tree in the future.
        public void ExampleOfExampleSpaces() => SnapshotGenExampleSpaces(G.Int32());
    }
}
