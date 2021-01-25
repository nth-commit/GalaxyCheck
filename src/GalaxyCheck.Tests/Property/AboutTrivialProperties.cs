using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck;
using System;
using Xunit;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Property
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryIterations) })]
    public class AboutTrivialProperties
    {
        [Property]
        public FsCheck.Property ATriviallyFalsePropertyIsFalsified(Iterations iterations)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var property = GC.Gen.Constant(false).ForAll(x => x);

                PropertyAssert.Falsifies(property, seed, iterations: iterations.Value);
            });

            return test.When(iterations.Value > 0);
        }

        [Property]
        public void ATriviallyTruePropertyIsNotFalsified(Iterations iterations) => TestWithSeed(seed =>
        {
            var property = GC.Gen.Constant(true).ForAll(x => x);

            var result = PropertyAssert.DoesNotFalsify(property, seed, iterations: iterations.Value);

            Assert.Equal(iterations.Value, result.Iterations);
        });
    }
}
