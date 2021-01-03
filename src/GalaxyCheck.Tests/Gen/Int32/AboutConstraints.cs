using Xunit;
using FsCheck.Xunit;
using FsCheck;
using System;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutConstraints
    {
        [Property]
        public void ItProducesValuesLessThanOrEqualMax(int max) => TestWithSeed(seed =>
        {
            var gen = G.Int32().LessThanEqual(max);

            var sample = gen.Sample(opts => opts.WithSeed(seed));

            Assert.All(sample, x => Assert.True(x <= max));
        });

        [Property]
        public void ItProducesValuesGreaterThanOrEqualMin(int min) => TestWithSeed(seed =>
        {
            var gen = G.Int32().GreaterThanEqual(min);

            var sample = gen.Sample(opts => opts.WithSeed(seed));

            Assert.All(sample, x => Assert.True(x >= min));
        });

        [Property]
        public Property ItProducesValuesBetweenRange(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().Between(min, max);

                var sample = gen.Sample(opts => opts.WithSeed(seed));

                Assert.All(sample, x => Assert.InRange(x, min, max));
            });

            return test.When(min <= max);
        }

        [Property]
        public void BetweenIsResilientToParameterOrdering(int x, int y) => TestWithSeed(seed =>
        {
            var gen0 = G.Int32().Between(x, y);
            var gen1 = G.Int32().Between(y, x);

            GenAssert.Equal(gen0, gen1, seed);
        });

        [Property]
        public Property BetweenIsEquivalentToGreaterThanEqualAndLessThenEqual(int min, int max)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen0 = G.Int32().GreaterThanEqual(min).LessThanEqual(max);
                var gen1 = G.Int32().Between(min, max);

                GenAssert.Equal(gen0, gen1, seed);
            });

            return test.When(min <= max);
        }
    }
}
