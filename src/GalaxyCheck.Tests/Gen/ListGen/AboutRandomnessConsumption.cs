using FsCheck;
using FsCheck.Xunit;
using System;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.ListGen
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryIterations) })]
    public class AboutRandomnessConsumption
    {
        [Property]
        public void ForRandomLengthLists_AndForAZeroFactorElementGen_ItConsumesRandomnessForTheLength(object element, NonZeroIterations iterations)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Constant(element).ListOf();

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);

                Assert.Equal(iterations.Value, sample.RandomnessConsumption);
            });
        }

        [Property]
        public void ForRandomLengthLists_AndForAOneFactorElementGen_ItConsumesRandomnessForTheLengthAndEachElement(NonZeroIterations iterations)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().ListOf();

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);

                var expectedRandomnessConsumption = sample.Values.Select(l => l.Count).Sum() + iterations.Value;
                Assert.Equal(expectedRandomnessConsumption, sample.RandomnessConsumption);
            });
        }

        [Property]
        public void ForSetLengthLists_AndForAZeroFactorElementGen_ItDoesNotConsumeRandomness(object element, NonZeroIterations iterations)
        {
            TestWithSeed(seed =>
            {
                var length = 5; // TODO: Arbitrary
                var gen = GC.Gen.Constant(element).ListOf().OfLength(length);

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed);

                Assert.Equal(0, sample.RandomnessConsumption);
            });
        }
    }
}
