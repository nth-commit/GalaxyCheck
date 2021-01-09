using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Random.Rng
{
    public class AboutNext
    {
        [Fact]
        public void ItIsIdempotent()
        {
            var rng = GC.Random.Rng.Spawn();

            Assert.Equal(rng.Next(), rng.Next());
        }

        [Property(StartSize = 0, EndSize = 100)]
        public FsCheck.Property ItIncrementsTheOrder(int seed, int nextCount)
        {
            Action test = () =>
            {
                var rng0 = GC.Random.Rng.Create(seed);

                var rngFinal = Enumerable.Range(0, nextCount).Aggregate(rng0, (acc, _) => acc.Next());

                Assert.Equal(nextCount, rngFinal.Order);
            };

            return test.When(nextCount >= 0);
        }

        [Fact]
        public void Examples()
        {
            var ranges = new List<(int min, int max)>
            {
                (0, 10),
                (0, 100),
                (0, 1000),
                (-10, 0),
                (-100, 0),
                (-1000, 0),
                (-10, 10),
                (-100, 100),
                (-1000, 1000)
            };

            SnapshotWithSeed(seed =>
                Enumerable
                    .Range(0, 10)
                    .Aggregate<int, IEnumerable<GC.IRng>>(
                        new[] { GC.Random.Rng.Create(seed) },
                        (acc, _) => Enumerable.Append(acc, acc.Last().Next()))
                    .ToArray());
        }
    }
}
