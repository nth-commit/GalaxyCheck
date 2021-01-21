using GalaxyCheck;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
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
        public static void TestWithSize(Action<int> testAction, int? repeatSize = null) =>
            Enumerable.Range(0, 11)
                .Select(x => x * 10)
                .Where(x => repeatSize == null || x == repeatSize)
                .ToList()
                .ForEach(testAction);

        public static List<T> WithSeed<T>(Func<int, T> fromSeed, int? repeatSeed = null)
        {
            return Enumerable.Range(0, 10).Where(x => repeatSeed == null || repeatSeed == x).Select(fromSeed).ToList();
        }

        public static void TestWithSeed(Action<int> testAction, int? repeatSeed = null) =>
            WithSeed(fromSeed: seed => seed, repeatSeed: repeatSeed).ForEach(seed =>
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

        public static void SnapshotWithSeed<T>(Func<int, T> fromSeed, int? repeatSeed = null, object[]? snapshotNameExtensions = null) =>
            TestWithSeed(
                seed => Snapshot.Match(
                    fromSeed(seed),
                    SnapshotNameExtension.Create(Enumerable
                        .Concat(
                            new object[] { seed },
                            snapshotNameExtensions ?? new object[] { })
                        .ToArray())),
                repeatSeed);

        public static void SnapshotGenValues<T>(IGen<T> gen, int? repeatSeed = null, int? size = null, Func<T, string>? format = null)
        {
            size ??= 100;
            format ??= x => x!.ToString()!;
            SnapshotWithSeed(
                fromSeed: seed => gen.Sample(seed: seed, size: size ?? 100).Select(x => format(x)),
                repeatSeed: repeatSeed,
                snapshotNameExtensions: new object[] { size });
        }

        public static void SnapshotGenExampleSpaces<T>(IGen<T> gen, int? repeatSeed = null, int? size = null, Func<T, string>? format = null)
        {
            size ??= 100;
            format ??= x => x!.ToString()!;
            SnapshotWithSeed(
                fromSeed: seed => gen
                    .Advanced
                    .SampleExampleSpaces(iterations: 1, seed: seed, size: size)
                    .Single()
                    .Render(x => format(x)),
                repeatSeed: repeatSeed,
                snapshotNameExtensions: new object[] { size });
        }
    }
}
