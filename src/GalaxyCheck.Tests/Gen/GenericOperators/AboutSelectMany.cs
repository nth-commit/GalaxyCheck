using FsCheck.Xunit;
using Xunit;
using System;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using GalaxyCheck.Internal.Random;
using System.Linq;

namespace Tests.Gen.GenericOperators
{
    [Properties(Arbitrary = new [] { typeof(ArbitraryGen) }, MaxTest = 10)]
    public class AboutSelectMany
    {
        [Fact]
        public void Snapshots()
        {
            var gen0 = GC.Gen.Int32().Between(0, 5);
            SnapshotGenExampleSpaces(
                from x in gen0
                from y in gen0
                select (x, y));
        }

        [Property(MaxTest = 1)]
        public void ItConsumesRandomnessAsMuchAsTheLeftAndRightGenerator(IGen<object> leftGen, IGen<object> rightGen)
        {
            TestWithSeed(seed =>
            {
                var gen = leftGen.SelectMany((_) => rightGen);

                var leftRandomness = leftGen.Advanced.SampleWithMetrics(seed: seed).RandomnessConsumption;
                var rightRandomness = rightGen.Advanced.SampleWithMetrics(seed: seed).RandomnessConsumption;

                var randomness = gen.Advanced
                    .SampleExampleSpacesWithMetrics(seed: seed)
                    .RandomnessConsumption;

                Assert.Equal(leftRandomness + rightRandomness, randomness);
            });
        }

        [Property]
        public void ItExhaustsWhenLeftGenHasImpossiblePredicate(IGen<object> leftGen, IGen<object> rightGen)
        {
            var gen =
                from x in leftGen
                where false
                from y in rightGen
                select (x, y);

            TestWithSeed(seed =>
            {
                Action test = () => gen.Sample(seed: seed);

                Assert.Throws<GC.Exceptions.GenExhaustionException>(test);
            });
        }

        [Property]
        public void ItExhaustsWhenRightGenHasImpossiblePredicate(IGen<object> leftGen, IGen<object> rightGen)
        {
            var gen =
                from x in leftGen
                from y in rightGen
                where false
                select (x, y);

            TestWithSeed(seed =>
            {
                Action test = () => gen.Sample(seed: seed);

                Assert.Throws<GC.Exceptions.GenExhaustionException>(test);
            });
        }
    }
}
