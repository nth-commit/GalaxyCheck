using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using R = GalaxyCheck.Random;
using static Tests.TestUtils;

namespace Tests.Random.Rng
{
    public class AboutValue
    {
        [Property]
        public FsCheck.Property ItIsIdempotent(int seed, int min, int max)
        {
            Action test = () =>
            {
                var rng = R.Rng.Create(seed);

                Assert.Equal(rng.Value(min, max), rng.Value(min, max));
            };

            return test.When(max >= min);
        }

        [Property]
        public FsCheck.Property ItThrowsWhenMinIsGreaterThanMax(int seed, int min, int max)
        {
            Action test = () =>
            {
                var rng = R.Rng.Create(seed);

                Assert.Throws<ArgumentOutOfRangeException>(() => rng.Value(min, max));
            };

            return test.When(min > max);
        }

        [Property]
        public void ItReturnsTheSinglePossibleValueWhenMinEqualsMax(int seed, int value)
        {
            var rng = R.Rng.Create(seed);

            Assert.Equal(value, rng.Value(value, value));
        }

        [Fact]
        public void Snapshots()
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
            {
                var rng = R.Rng.Create(seed);

                return ranges
                    .Select((range) => new { range.min, range.max, value = rng.Value(range.min, range.max) })
                    .ToList();
            });
        }
    }
}
