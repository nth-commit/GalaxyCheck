using FsCheck;
using FsCheck.Xunit;
using Snapshooter;
using Snapshooter.Xunit;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using R = GalaxyCheck.Random;

namespace GalaxyCheck.Tests.Random.Rng
{
    public class AboutValue
    {
        [Property]
        public Property ItIsIdempotent(int seed, int min, int max)
        {
            Action test = () =>
            {
                var rng = R.Rng.Create(seed);

                Assert.Equal(rng.Value(min, max), rng.Value(min, max));
            };

            return test.When(max >= min);
        }

        [Property]
        public Property ItThrowsWhenMinIsGreaterThanMax(int seed, int min, int max)
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

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void Examples(int seed)
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

            var rng = R.Rng.Create(seed);

            Snapshot.Match(
                ranges
                    .Select((range) => new { range.min, range.max, value = rng.Value(range.min, range.max) })
                    .ToList(),
                SnapshotNameExtension.Create(seed));
        }
    }
}
