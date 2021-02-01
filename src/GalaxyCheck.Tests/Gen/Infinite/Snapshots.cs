using FsCheck;
using Newtonsoft.Json;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Infinite
{
    public class Snapshots
    {
        [Fact]
        public void Snapshot_Infinite_Of_0_To_10_Taken_3_Times()
        {
            var infiniteGen = GC.Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(source => source.Take(3).ToList());

            SnapshotGenExampleSpaces(infiniteGen, format: xs => JsonConvert.SerializeObject(xs.Take(3)));
        }
    }
}
