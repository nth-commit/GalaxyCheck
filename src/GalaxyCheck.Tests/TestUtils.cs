﻿using GalaxyCheck;
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
        public static void TestWithSeed(Action<int> testAction, int? repeatSeed = null) =>
            Enumerable.Range(0, 10).Where(x => repeatSeed == null || repeatSeed == x).ToList().ForEach(seed =>
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

        public static void SnapshotWithSeed<T>(Func<int, T> fromSeed, int? repeatSeed = null) =>
            TestWithSeed(seed => Snapshot.Match(fromSeed(seed), SnapshotNameExtension.Create(seed)), repeatSeed);

        public static void SnapshotGenValues<T>(IGen<T> gen, int? repeatSeed = null) =>
            SnapshotWithSeed(seed => gen.Sample(new RunConfig(seed: seed, size: GalaxyCheck.Sizing.Size.MaxValue)), repeatSeed);

        public static void SnapshotGenExampleSpaces<T>(IGen<T> gen, int? repeatSeed = null) =>
            SnapshotWithSeed(
                seed => gen
                    .Advanced
                    .SampleExampleSpaces(new RunConfig(iterations: 1, seed: seed, size: GalaxyCheck.Sizing.Size.MaxValue))
                    .Single()
                    .Render(x => x!.ToString()!),
                repeatSeed);
    }
}
