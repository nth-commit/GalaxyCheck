using Snapshooter;
using Snapshooter.Xunit;
using System;

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
    }
}
