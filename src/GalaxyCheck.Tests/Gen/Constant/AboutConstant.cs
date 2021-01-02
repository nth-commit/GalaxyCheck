using FsCheck.Xunit;
using GalaxyCheck.Runners;
using Xunit;
using G = GalaxyCheck.Gen;

namespace GalaxyCheck.Tests.Gen.Constant
{
    public class AboutConstant
    {
        [Property]
        public void ItProducesTheGivenValue(object value)
        {
            var gen = G.Constant(value);

            var sample = gen.Sample();

            Assert.Equal(100, sample.Count);
            Assert.All(sample, genValue =>
            {
                Assert.Equal(value, genValue);
            });
        }

        [Property]
        public void ItDoesNotConsumeRandomness(object value)
        {
            var gen = G.Constant(value);

            var sample = gen.SampleWithMetrics();

            Assert.Equal(0, sample.RandomConsumptionCount);
        }
    }
}
