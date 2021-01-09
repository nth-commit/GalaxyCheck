using Xunit;
using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.GenericOperators
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen) })]
    public class AboutSelect
    {
        [Fact]
        public void Snapshots() => SnapshotGenExampleSpaces(GC.Gen.Int32().Between(65, 90).Select(x => ((char)x).ToString()));

        [Property]
        public void LinqSupport(GC.IGen<object> gen)
        {
            var _ =
                from x in gen
                select x;
        }
    }
}
