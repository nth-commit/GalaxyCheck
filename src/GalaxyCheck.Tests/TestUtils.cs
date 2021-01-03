using GalaxyCheck.Abstractions;
using GalaxyCheck.Aggregators;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Linq;

namespace GalaxyCheck.Tests
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
            SnapshotWithSeed(seed => gen.Sample(opts => opts.WithSeed(seed)));

        public static void SnapshotGenExampleSpaces<T>(IGen<T> gen) =>
            SnapshotWithSeed(seed =>
            {
                return gen
                    .SampleExampleSpaces(opts => opts.WithSeed(seed).WithIterations(1))
                    .Single()
                    .Render(x => x.ToString());
            });
    }
}
