using Xunit;
using FsCheck.Xunit;
using FsCheck;
using System;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    public class AboutConstraints
    {
        [Property]
        public FsCheck.Property ItProducesValuesGreaterThanOrEqualMin(int min, int max, int origin, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32()
                    .GreaterThanEqual(min)
                    .LessThanEqual(max)
                    .ShrinkTowards(origin)
                    .WithBias(bias);

                var sample = gen.Sample(new RunConfig(seed: seed));

                Assert.All(sample, x => Assert.True(x >= min));
            });

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public FsCheck.Property ItProducesValuesLessThanOrEqualMax(int min, int max, int origin, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32()
                    .GreaterThanEqual(min)
                    .LessThanEqual(max)
                    .ShrinkTowards(origin)
                    .WithBias(bias);

                var sample = gen.Sample(new RunConfig(seed: seed));

                Assert.All(sample, x => Assert.True(x <= max));
            });

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public FsCheck.Property ItProducesValuesBetweenRange(int min, int max, int origin, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32()
                    .Between(min, max)
                    .ShrinkTowards(origin)
                    .WithBias(bias);

                var sample = gen.Sample(new RunConfig(seed: seed));

                Assert.All(sample, x => Assert.InRange(x, min, max));
            });

            return test.When(min <= origin && origin <= max);
        }

        [Property]
        public void BetweenIsResilientToParameterOrdering(int x, int y) => TestWithSeed(seed =>
        {
            var gen0 = GC.Gen.Int32().Between(x, y);
            var gen1 = GC.Gen.Int32().Between(y, x);

            GenAssert.Equal(gen0, gen1, seed);
        });

        [Property]
        public FsCheck.Property BetweenIsEquivalentToGreaterThanEqualAndLessThenEqual(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = GC.Gen.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = GC.Gen.Int32().Between(min, max);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= max);
        }
    }
}
