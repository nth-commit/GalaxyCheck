using GalaxyCheck.Abstractions;
using GalaxyCheck.Aggregators;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Linq;

namespace GalaxyCheck.Tests
{
    public static class TestUtils
    {
        public static void SnapshotWithSeed<T>(Func<int, T> fromSeed)
        {
            for (var seed = 0; seed < 10; seed++)
            {
                Snapshot.Match(fromSeed(seed), SnapshotNameExtension.Create(seed));
            }
        }

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
