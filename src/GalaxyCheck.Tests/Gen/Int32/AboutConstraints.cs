using Xunit;
using FsCheck.Xunit;
using FsCheck;
using System;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    /// <summary>
    /// TODO: When porting these tests to the V2 suite using NebulaCheck, we should also be testing that the shrinks
    /// are constrained in the same way the root values are.
    /// </summary>
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

                var sample = gen.Sample(seed: seed);

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

                var sample = gen.Sample(seed: seed);

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

                var sample = gen.Sample(seed: seed);

                Assert.All(sample, x => Assert.InRange(x, min, max));
            });

            return test.When(min <= origin && origin <= max);
        }
    }
}
