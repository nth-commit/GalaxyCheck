using Xunit;
using Newtonsoft.Json;
using GC = GalaxyCheck;
using GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.ListGen
{
    public class Snapshots
    {
        [Fact]
        public void Example() => SnapshotGenValues(GC.Gen.Int32().ListOf(), format: xs => JsonConvert.SerializeObject(xs));

        [Fact]
        public void ExampleOfExampleSpaces() => SnapshotGenExampleSpaces(
            GC.Gen.Int32().Between(0, 5).ListOf().BetweenLengths(0, 5),
            format: xs => JsonConvert.SerializeObject(xs));
    }
}
