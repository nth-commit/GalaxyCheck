using System;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;
using FsCheck.Xunit;
using FsCheck;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutDefaults
    {
        [Property]
        public Property TheDefaultMinIsTheMinInt32(int max, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = G.Int32().LessThanEqual(max).ShrinkTowards(origin);
                var gen1 = gen0.GreaterThanEqual(int.MinValue);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(max >= origin);
        }

        [Property]
        public Property TheDefaultMaxIsTheMaxInt32(int min, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = G.Int32().GreaterThanEqual(min).ShrinkTowards(origin);
                var gen1 = gen0.LessThanEqual(int.MaxValue);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= origin);
        }

        [Property]
        public Property WhenMinIsLessThanEqualZeroAndMaxIsGreaterThanEqualZero_TheDefaultOriginIsZero(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = G.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(0);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= 0 && 0 <= max);
        }

        [Property]
        public Property WhenMinIsGreaterThanZero_TheDefaultOriginIsMin(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = G.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(min);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min > 0 && max >= min);
        }

        [Property]
        public Property WhenMaxIsLessThanZero_TheDefaultOriginIsMax(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = G.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = gen0.ShrinkTowards(max);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(max < 0 && max >= min);
        }
    }
}
