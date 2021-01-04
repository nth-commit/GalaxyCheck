using Xunit;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;
using System.Linq;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutDistribution
    {
        [Fact]
        public void ItHasAnEvenDistribution() => TestWithSeed(seed =>
        {
            var gen = G.Int32().Between(0, 10);

            var values = gen.Sample(opts => opts.WithSeed(seed).WithIterations(1000));
            var mean = values.Average();

            Assert.Equal(5, mean, 0);
        });
    }
}
