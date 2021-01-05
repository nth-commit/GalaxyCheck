using Xunit;
using G = GalaxyCheck.Gen;
using S = GalaxyCheck.Sizing;
using static GalaxyCheck.Tests.TestUtils;
using System.Linq;
using FsCheck.Xunit;
using GalaxyCheck.Abstractions;

namespace GalaxyCheck.Tests.Gen.Int32
{
    [Properties(Arbitrary = new[] { typeof(ArbitrarySize) }, MaxTest = 10)]
    public class AboutDistribution
    {
        [Property]
        public void WhenBiasIsNone_ItHasAnEvenDistribution(ISize size) => TestWithSeed(seed =>
        {
            var gen = G.Int32().Between(0, 100).WithBias(G.Bias.None);

            var values = gen.Sample(opts => opts.WithSeed(seed).WithIterations(10000).WithSize(size));
            var mean = values.Average();

            Assert.Equal(50, mean, 0);
        });

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsMin_ItProducesValuesOnlyOfTheMin()
        {
            TestWithSeed(seed =>
            {
                var gen = G.Int32().Between(0, 100).WithBias(G.Bias.Linear);

                var values = gen.Sample(opts => opts.WithSeed(seed).WithIterations(10000).WithSize(S.Size.MinValue));

                Assert.All(values, x => Assert.Equal(0, x));
            });
        }

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsHalf_ItHasAnEvenDistributionOverLowerHalfOfRange()
        {
            TestWithSeed(seed =>
            {
                var gen = G.Int32().Between(0, 100).WithBias(G.Bias.Linear);

                var values = gen.Sample(opts => opts.WithSeed(seed).WithIterations(10000).WithSize(new S.Size(50)));
                var mean = values.Average();

                Assert.Equal(25, mean, 0);
            });
        }

        [Fact]
        public void WhenBiasIsLinear_AndWhenSizeIsMax_ItHasAnEvenDistributionOverWholeRange()
        {
            TestWithSeed(seed =>
            {
                var gen = G.Int32().Between(0, 100).WithBias(G.Bias.Linear);

                var values = gen.Sample(opts => opts.WithSeed(seed).WithIterations(10000).WithSize(S.Size.MaxValue));
                var mean = values.Average();

                Assert.Equal(50, mean, 0);
            });
        }
    }
}
