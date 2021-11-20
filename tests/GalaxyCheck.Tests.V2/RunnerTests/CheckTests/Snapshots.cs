using GalaxyCheck;
using NebulaCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System.Collections.Immutable;
using System.Linq;
using Xunit;


namespace Tests.V2.RunnerTests.CheckTests
{
    public class Snapshots
    {
        [Fact]
        public void Unfalsifiable() => RunSnapshots(GalaxyCheck.Gen.Int32().ForAll((x) => true));

        [Fact]
        public void Unfalsifiable_Filtered() => RunSnapshots(GalaxyCheck.Gen.Int32().Where(x => x % 2 == 0).ForAll((x) => true));

        [Fact]
        public void Falsifiable_AtLargerSizes() => RunSnapshots(GalaxyCheck.Gen.Create(parameters => (parameters.Size.Value, parameters)).ForAll(x => x < 50));

        [Fact]
        public void Falsifiable_AtSmallerSizes() => RunSnapshots(GalaxyCheck.Gen.Create(parameters => (parameters.Size.Value, parameters)).ForAll(x => x > 50));

        private static void RunSnapshots(GalaxyCheck.Property<int> property)
        {
            var seed = 0;

            foreach (var iterations in new[] { 1, 2, 50, 100 })
            {
                var check = property.Check(seed: seed, iterations: iterations);

                Snapshot.Match(
                    check with { Checks = ImmutableList.Create<GalaxyCheck.Runners.Check.CheckIteration<int>>() },
                    SnapshotNameExtension.Create($"Iterations={iterations}"));
            }
        }
    }
}
