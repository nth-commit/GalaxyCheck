using FsCheck.Xunit;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Property.Check
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen), typeof(ArbitraryIterations) })]
    public class AboutIterations
    {
        [Property]
        public void ItCallsTheTestFunctionForEachIteration(IGen<object> gen, Iterations iterations)
        {
            TestWithSeed(seed =>
            {
                var calls = 0;
                var property = gen.ForAll(_ =>
                {
                    calls++;
                    return true;
                });

                property.Check(iterations: iterations.Value, seed: seed);

                Assert.Equal(iterations.Value, calls);
            });
        }

        [Property]
        public void ItReturnsTheGivenIterations(IGen<object> gen, Iterations iterations)
        {
            TestWithSeed(seed =>
            {
                var property = gen.ForAll(_ => true);

                var result = property.Check(iterations: iterations.Value, seed: seed);

                Assert.Equal(iterations.Value, result.Iterations);
            });
        }
    }
}
