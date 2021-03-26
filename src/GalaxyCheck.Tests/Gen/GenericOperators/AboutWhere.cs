using FsCheck.Xunit;
using System;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.GenericOperators
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen), typeof(ArbitraryIterations) }, MaxTest = 10)]
    public class AboutWhere
    {
        [Fact]
        public void Snapshots() => SnapshotGenExampleSpaces(GC.Gen.Int32().Between(0, 100).Where(x => x % 2 == 0));

        [Fact]
        public void ItProducesValuesThatPassThePredicate() => TestWithSeed(seed =>
        {
            Func<int, bool> pred = x => x % 2 == 0;
            var gen = GC.Gen.Int32().Where(pred);

            var values = gen.Sample(seed: seed);

            Assert.All(values, x => Assert.True(pred(x)));
        });

        [Fact]
        public void ItProducesExampleSpacesThatPassThePredicate() => TestWithSeed(seed =>
        {
            Func<int, bool> pred = x => x % 2 == 0;
            var gen = GC.Gen.Int32().Where(pred);

            var exampleSpaces = gen.Advanced.SampleExampleSpaces(seed: seed);

            Assert.All(exampleSpaces, exampleSpace =>
            {
                Assert.All(exampleSpace.Sample(), example =>
                {
                    Assert.True(pred(example.Value));
                });
            });
        });

        [Property]
        public void WhenThePredicateOnlyPassesForLargerSizes_ItStillProducesValues(Iterations iterations)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Advanced.Primitive((useNextInt, size) => size.Value).Where(size => size > 10);

                var values = gen.Sample(
                    seed: seed,
                    size: 0,
                    iterations: iterations.Value);

                Assert.True(values.Count == iterations.Value);
                Assert.All(values, x => Assert.True(x > 10));
            });
        }
    }
}
