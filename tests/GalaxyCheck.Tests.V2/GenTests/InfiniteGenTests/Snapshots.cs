using GalaxyCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using System.Text.Json;
using Xunit;

namespace Tests.V2.GenTests.InfiniteGenTests
{
    public class Snapshots
    {
        [Fact]
        public void Snapshot_Infinite_Of_0_To_10_Taken_3_Times()
        {
            var seeds = Enumerable.Range(0, 3);

            var gen = Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(source => source.Take(3).ToList());

            foreach (var seed in seeds)
            {
                var sample = gen.Advanced
                    .SampleOneExampleSpace(seed: seed, size: 75)
                    .Take(500)
                    .Render(x => JsonSerializer.Serialize(x));

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
