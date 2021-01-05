using Xunit;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.GenericOperators
{
    public class AboutSelect
    {
        [Fact]
        public void Snapshots() => SnapshotGenExampleSpaces(G.Int32().Between(65, 90).Select(x => ((char)x).ToString()));

        [Fact]
        public void LinqSupport()
        {
            var _ =
                from x in G.Int32()
                select x;
        }
    }
}
