using Xunit;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutProductionOfValues
    {
        [Fact]
        public void Example() => SnapshotWithSeed(seed => G.Int32().Sample(opts => opts.WithSeed(seed)));
    }
}
