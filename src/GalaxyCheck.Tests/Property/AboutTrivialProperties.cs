using FsCheck;
using FsCheck.Xunit;
using GalaxyCheck;
using System;
using Xunit;
using GC = GalaxyCheck;

namespace Tests.Property
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryIterations) })]
    public class AboutTrivialProperties
    {
        [Property]
        public FsCheck.Property ATriviallyFalsePropertyIsFalsified(Iterations iterations)
        {
            Action test = () =>
            {
                var gen = GC.Gen.Constant(false);
                var property = GC.Property.ForAll(gen, x => x);

                var result = property.Check(new RunConfig(iterations: iterations.Value));

                Assert.True(result is CheckResult.Falsified<bool>);
                Assert.Equal(1, result.Runs);
            };

            return test.When(iterations.Value > 0);
        }

        [Property]
        public void ATriviallyTruePropertyIsNotFalsified(Iterations iterations)
        {
            var gen = GC.Gen.Constant(true);
            var property = GC.Property.ForAll(gen, x => x);

            var result = property.Check(new RunConfig(iterations: iterations.Value));

            Assert.True(result is CheckResult.Unfalsified<bool>);
            Assert.Equal(iterations.Value, result.Runs);
        }
    }
}
