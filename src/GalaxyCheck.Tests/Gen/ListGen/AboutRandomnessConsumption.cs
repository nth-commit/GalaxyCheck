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
    [Properties(Arbitrary = new[] { typeof(ArbitraryIterations), typeof(ArbitrarySize) })]
    public class AboutRandomnessConsumption
    {
        [Property]
        public FsCheck.Property ForRandomLengthLists_AndForAZeroFactorElementGen_ItConsumesRandomnessForTheLength(
            object element,
            Size size,
            NonZeroIterations iterations)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Constant(element).ListOf();

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed, size: size.Value);

                Assert.Equal(iterations.Value, sample.RandomnessConsumption);
            });

            // The integer generator which generates the length does weird shit (intentionally) at sizes above 50.
            return test.When(size.Value <= 50);
        }

        [Property]
        public FsCheck.Property ForRandomLengthLists_AndForAOneFactorElementGen_ItConsumesRandomnessForTheLengthAndEachElement(
            Size size,
            NonZeroIterations iterations)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().ListOf();

                var sample = gen.Advanced.SampleWithMetrics(iterations: iterations.Value, seed: seed, size: size.Value);

                var expectedRandomnessConsumption = sample.Values.Select(l => l.Count).Sum() + iterations.Value;
                Assert.Equal(expectedRandomnessConsumption, sample.RandomnessConsumption);
            });

            // The integer generator which generates the length does weird shit (intentionally) at sizes above 50.
            return test.When(size.Value <= 50);
        }

        [Property]
        public void ForSetLengthLists_AndForAZeroFactorElementGen_ItDoesNotConsumeRandomness(
            object element,
            NonZeroIterations iterations)
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
