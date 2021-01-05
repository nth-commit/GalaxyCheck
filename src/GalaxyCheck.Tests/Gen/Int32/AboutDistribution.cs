using Xunit;
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
        public void WhenBiasIsNone_ItHasAnEvenDistribution(GC.Abstractions.ISize size) => TestWithSeed(seed =>
        {
            var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.None);

            var values = gen.Sample(new RunConfig(iterations: 10000, seed: seed, size: size));
            var mean = values.Average();

            Assert.Equal(50, mean, 0);
        });

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsMin_ItProducesValuesOnlyOfTheMin()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Linear);

                var values = gen.Sample(new RunConfig(iterations: 10000, seed: seed, size: GC.Sizing.Size.MinValue));

                Assert.All(values, x => Assert.Equal(0, x));
            });
        }

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsHalf_ItHasAnEvenDistributionOverLowerHalfOfRange()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(0, 100).WithBias(GC.Gen.Bias.Linear);

                var values = gen.Sample(new RunConfig(iterations: 10000, seed: seed, size: new GC.Sizing.Size(50)));
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

                var values = gen.Sample(new RunConfig(iterations: 10000, seed: seed, size: GC.Sizing.Size.MaxValue));
                var mean = values.Average();

                Assert.Equal(50, mean, 0);
            });
        }
    }
}
