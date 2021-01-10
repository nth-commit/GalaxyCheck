using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.GenericOperators
{
    public class AboutBind
    {
        [Fact]
        public void Snapshots()
        {
            var gen0 = GC.Gen.Int32().Between(0, 5);
            SnapshotGenExampleSpaces(gen0.Bind(x => gen0.Select(y => (x, y))));
        }
    }
}
