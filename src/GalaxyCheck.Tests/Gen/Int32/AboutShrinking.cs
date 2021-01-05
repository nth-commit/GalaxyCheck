using FsCheck;
using FsCheck.Xunit;
using System;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    public class AboutShrinking
    {
        [Property]
        public FsCheck.Property ItShrinksTowardsTheSuppliedOrigin(int min, int max, int origin, GC.Gen.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .GreaterThanEqual(min)
                    .LessThanEqual(max)
                    .ShrinkTowards(origin)
                    .WithBias(bias);

                GenAssert.ShrinksTo(gen, origin, seed);
            });

            return test.When(min <= origin && origin <= max);
        }
    }
}
