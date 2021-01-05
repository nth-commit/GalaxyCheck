using FsCheck.Xunit;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;

namespace Tests.Gen.Constant
{
    public class AboutConstant
    {
        [Property]
        public void ItProducesTheGivenValue(object value)
        {
            var gen = GC.Gen.Constant(value);

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
            var gen = GC.Gen.Constant(value);

            var sample = gen.SampleWithMetrics();

            Assert.Equal(0, sample.RandomnessConsumption);
        }
    }
}
