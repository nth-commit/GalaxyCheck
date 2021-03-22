using GalaxyCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Linq;
using Xunit;

namespace Tests.V2.GenTests.ChooseGenTests
{
    public class Snapshots
    {
        [Fact]
        public void Snapshot_Positive_Numbers_Five_Times_As_Likely()
        {
            var seeds = Enumerable.Range(0, 5);

            foreach (var seed in seeds)
            {
                var gen = Gen.Choose<int>()
                    .WithChoice(Gen.Int32().GreaterThan(0), 5)
                    .WithChoice(Gen.Int32().LessThan(0), 1);

                var sample = gen.Sample(seed: seed);

                var nameExtension = string.Join("_", new[]
                {
                    $"Seed_{seed}"
                });

                Snapshot.Match(sample, SnapshotNameExtension.Create(nameExtension));
            }
        }
    }
}
