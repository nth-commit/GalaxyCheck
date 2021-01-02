using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck.Abstractions;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using R = GalaxyCheck.Random;

namespace GalaxyCheck.Tests.Random.Rng
{
    public class AboutNext
    {
        [Fact]
        public void ItIsIdempotent()
        {
            var rng = R.Rng.Spawn();

            Assert.Equal(rng.Next(), rng.Next());
        }

        [Property(StartSize = 0, EndSize = 100)]
        public Property ItIncrementsTheOrder(int seed, int nextCount)
        {
            Action test = () =>
            {
                var rng0 = R.Rng.Create(seed);

                var rngFinal = Enumerable.Range(0, nextCount).Aggregate(rng0, (acc, _) => acc.Next());

                Assert.Equal(nextCount, rngFinal.Order);
            };

            return test.When(nextCount >= 0);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Example(int seed)
        {
            var rngs = Enumerable
                .Range(0, 10)
                .Aggregate<int, IEnumerable<IRng>>(
                    new[] { R.Rng.Create(seed) },
                    (acc, _) => Enumerable.Append(acc, acc.Last().Next()))
                .ToArray();

            Snapshot.Match(rngs, SnapshotNameExtension.Create(seed));
        }
    }
}
