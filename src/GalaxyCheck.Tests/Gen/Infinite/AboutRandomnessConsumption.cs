using FsCheck.Xunit;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Infinite
{
    [Properties(Arbitrary = new[] { typeof(ArbitrarySize), typeof(ArbitraryIterations) }, MaxTest = 10)]
    public class AboutRandomnessConsumption
    {
        [Property]
        public void ItConsumesRandomnessOncePerIteration(Iterations iterations, Size size)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().InfiniteOf();

                var sample = gen.Advanced.SampleWithMetrics(
                    iterations: iterations.Value,
                    seed: seed,
                    size: size.Value);

                Assert.Equal(iterations.Value, sample.RandomnessConsumption);
            });
        }
    }
}
