﻿using Xunit;
using System.Linq;
using FsCheck.Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    [Properties(Arbitrary = new[] { typeof(ArbitrarySize) }, MaxTest = 10)]
    public class AboutDistribution
    {
        [Property]
        public void WhenBiasIsNone_ItHasAnEvenDistribution(Size size) => TestWithSeed(seed =>
        {
            var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.None);

            var values = gen.Sample(iterations: 10000, seed: seed, size: size.Value);
            var mean = values.Average();

            Assert.Equal(50, mean, 0);
        });

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsMin_ItProducesValuesOnlyOfTheMin()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Linear);

                var values = gen.Sample(iterations: 10000, seed: seed, size: 0);

                Assert.All(values, x => Assert.Equal(0, x));
            });
        }

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsHalf_ItHasAnEvenDistributionOverLowerHalfOfRange()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Linear);

                var values = gen.Sample(iterations: 10000, seed: seed, size: 50);
                var mean = values.Average();

                Assert.Equal(25, mean, 0);
            });
        }

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsMax_ItHasAnEvenDistributionOverWholeRange()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Linear);

                var values = gen.Sample(iterations: 10000, seed: seed, size: 100);
                var mean = values.Average();

                Assert.Equal(50, mean, 0);
            });
        }

        [Fact]
        public void WhenBiasIsExponential_AndWhenSizeIsMin_ItProducesValuesOnlyOfTheMin()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Exponential);

                var values = gen.Sample(iterations: 10000, seed: seed, size: 0);

                Assert.All(values, x => Assert.Equal(0, x));
            });
        }

        [Fact]
        public void WhenBiasIsExponential_AndWhenSizeIsHalf_ItHasAnEvenDistributionOverLowerHalfOfExponentialRange()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Exponential);

                var values = gen.Sample(iterations: 10000, seed: seed, size: 50);
                var mean = values.Average();

                Assert.Equal(5, mean, 0);
            });
        }

        [Fact]
        public void WhenBiasIsExponential_AndWhenSizeIsMax_ItHasAnEvenDistributionOverWholeRange()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Exponential);

                var values = gen.Sample(iterations: 10000, seed: seed, size: 100);
                var mean = values.Average();

                Assert.Equal(50, mean, 0);
            });
        }
    }
}
