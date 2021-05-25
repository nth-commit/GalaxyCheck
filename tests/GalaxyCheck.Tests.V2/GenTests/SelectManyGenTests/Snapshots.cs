using GalaxyCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.SelectManyGenTests
{
    public class Snapshots
    {
        [Fact]
        public void ExampleSpaces()
        {
            var seeds = Enumerable.Range(0, 3);

            var gen =
                from x in Gen.Int32().Between(0, 10)
                from y in Gen.Int32().Between(x, x + 10)
                select (x, y);

            foreach (var seed in seeds)
            {
                var sample = gen.RenderOneTraversal(seed: seed, size: 75);

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
