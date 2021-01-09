using GalaxyCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Linq;

namespace Tests
{
    public class FailedForSeedException : Exception
    {
        public int Seed { get; }

        public FailedForSeedException(Exception innerException, int seed)
            : base($"Failed for seed {seed}", innerException)
        {
            Seed = seed;
        }
    }

    public static class TestUtils
    {
        public static void TestWithSeed(Action<int> testAction) =>
            Enumerable.Range(0, 10).ToList().ForEach(seed =>
            {
                try
                {
                    testAction(seed);
                }
                catch (Exception ex)
                {
                    throw new FailedForSeedException(ex, seed);
                }
            });

        public static void SnapshotWithSeed<T>(Func<int, T> fromSeed) =>
            TestWithSeed(seed => Snapshot.Match(fromSeed(seed), SnapshotNameExtension.Create(seed)));

        public static void SnapshotGenValues<T>(IGen<T> gen) =>
            SnapshotWithSeed(seed => gen.Sample(new RunConfig(seed: seed, size: GalaxyCheck.Sizing.Size.MaxValue)));

        public static void SnapshotGenExampleSpaces<T>(IGen<T> gen) =>
            SnapshotWithSeed(seed =>
            {
                return gen
                    .Advanced
                    .SampleExampleSpaces(new RunConfig(iterations: 1, seed: seed, size: GalaxyCheck.Sizing.Size.MaxValue))
                    .Single()
                    .Render(x => x!.ToString()!);
            });
    }
}
