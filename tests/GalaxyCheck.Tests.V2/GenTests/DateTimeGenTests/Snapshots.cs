using GalaxyCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.DateTimeGenTests
{
    public class Snapshots
    {
        [Fact]
        public void Values()
        {
            var sample = Gen
                .DateTime()
                .Select(x => x.ToString("s"))
                .Sample(seed: 0);

            var nameExtension = string.Join("_", new[]
            {
                $"Seed_{0}"
            });

            Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
        }

        [Fact]
        public void ExampleSpaces()
        {
            var seeds = Enumerable.Range(0, 3);

            foreach (var seed in seeds)
            {
                var sample = Gen.DateTime().RenderOneTraversal(
                    seed: seed,
                    size: 75,
                    renderer: x => x.ToString("s"));

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
