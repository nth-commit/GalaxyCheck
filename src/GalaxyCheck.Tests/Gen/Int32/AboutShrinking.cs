using FsCheck;
using FsCheck.Xunit;
using System;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutShrinking
    {
        [Property]
        public Property ItShrinksTowardsTheSuppliedOrigin(int min, int max, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().GreaterThanEqual(min).LessThanEqual(max).ShrinkTowards(origin);

                GenAssert.ShrinksTo(gen, origin, seed);
            });

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public Property WhenMinIsLessThanEqualZeroAndMaxIsGreaterThanEqualZero_ItShrinksTowardsZero(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().GreaterThanEqual(min).LessThanEqual(max);

                GenAssert.ShrinksTo(gen, 0, seed);
            });

            return test.When(min <= 0 && 0 <= max);
        }

        [Property]
        public Property WhenMinIsGreaterThanZero_ItShrinksTowardsMin(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().GreaterThanEqual(min).LessThanEqual(max);

                GenAssert.ShrinksTo(gen, min, seed);
            });

            return test.When(min > 0 && max >= min);
        }

        [Property]
        public Property WhenMaxIsLessThanZero_ItShrinksTowardsMax(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().GreaterThanEqual(min).LessThanEqual(max);

                GenAssert.ShrinksTo(gen, max, seed);
            });

            return test.When(max < 0 && max >= min);
        }
    }
}
