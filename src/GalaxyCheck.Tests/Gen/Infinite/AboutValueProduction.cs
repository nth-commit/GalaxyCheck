using FsCheck;
using FsCheck.Xunit;
using System.Linq;
using System.Collections.Immutable;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Infinite
{
    [Properties(MaxTest = 10, Arbitrary = new [] { typeof(ArbitraryIterations), typeof(ArbitrarySize) })]
    public class AboutValueProduction
    {
        [Property]
        public void ItProducesTheSameElementsForEachEnumeration(Size size)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().InfiniteOf();

                var sample = gen.SampleOne(seed: seed, size: size.Value);

                Assert.Equal(sample.Take(10), sample.Take(10));
            });
        }

        [Property]
        public void ItProducesElementsLikeList(Size size, NonNegativeInt l)
        {
            var length = l.Get % 20;

            /*
             * The generation is mostly comparable, except:
             *  - We have to give the list generator a forked seed, to simulate the forking that happens in the
             *    infinite enumerable.
             *  - We have to set the list's length to be a specific value using OfLength, as that short-circuits
             *    consuming randomness for generating the list's length.
             */

            TestWithSeed(seed =>
            {
                var elementGen = GC.Gen.Int32();

                var listSample = elementGen
                    .ListOf()
                    .OfLength(length)
                    .SampleOne(seed: ForkSeed(seed), size: size.Value);

                var infiniteSample = elementGen
                    .InfiniteOf()
                    .Select(enumerable => enumerable.Take(listSample.Count).ToImmutableList())
                    .SampleOne(seed: seed, size: size.Value);

                Assert.Equal(listSample, infiniteSample);
            });
        }

        private static int ForkSeed(int seed)
        {
            return GC.Internal.Random.Rng.Create(seed).Fork().Seed;
        }
    }
}
