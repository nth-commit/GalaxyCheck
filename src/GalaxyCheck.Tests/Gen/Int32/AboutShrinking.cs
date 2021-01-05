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
        public Property ItShrinksTowardsTheSuppliedOrigin(int min, int max, int origin, G.Bias bias)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().GreaterThanEqual(min).LessThanEqual(max).ShrinkTowards(origin).WithBias(bias);

                GenAssert.ShrinksTo(gen, origin, seed);
            });

            return test.When(min <= origin && origin <= max);
        }
    }
}
