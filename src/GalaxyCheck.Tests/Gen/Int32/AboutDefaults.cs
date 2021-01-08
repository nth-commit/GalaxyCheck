using System;
using FsCheck.Xunit;
using FsCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    [Properties(MaxTest = 10)]
    public class AboutDefaults
    {
        [Property]
        public FsCheck.Property TheDefaultMinIsTheMinInt32(int max, int origin, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().LessThanEqual(max).ShrinkTowards(origin).WithBias(bias);
                var gen1 = gen0.GreaterThanEqual(int.MinValue);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(max >= origin);
        }

        [Property]
        public FsCheck.Property TheDefaultMaxIsTheMaxInt32(int min, int origin, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().GreaterThanEqual(min).ShrinkTowards(origin).WithBias(bias);
                var gen1 = gen0.LessThanEqual(int.MaxValue);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= origin);
        }

        [Property]
        public FsCheck.Property WhenMinIsLessThanEqualZeroAndMaxIsGreaterThanEqualZero_TheDefaultOriginIsZero(int min, int max, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max).WithBias(bias);
                var gen1 = gen0.ShrinkTowards(0);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= 0 && 0 <= max);
        }

        [Property]
        public FsCheck.Property WhenMinIsGreaterThanZero_TheDefaultOriginIsMin(int min, int max, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max).WithBias(bias);
                var gen1 = gen0.ShrinkTowards(min);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min > 0 && max >= min);
        }

        [Property]
        public FsCheck.Property WhenMaxIsLessThanZero_TheDefaultOriginIsMax(int min, int max, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max).WithBias(bias);
                var gen1 = gen0.ShrinkTowards(max);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(max < 0 && max >= min);
        }

        [Property]
        public FsCheck.Property TheDefaultBiasIsLinear(int min, int max, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max).ShrinkTowards(origin);
                var gen1 = gen0.WithBias(GC.Gen.Bias.Linear);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= origin && origin <= max);
        }
    }
}
