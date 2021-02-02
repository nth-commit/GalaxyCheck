using FsCheck;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Infinite
{
    public class AboutShrinking
    {
        [Fact]
        public void WhenItIsNotEnumerated_ItDoesNotShrink() => TestWithSeed(seed =>
        {
            var gen = GC.Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(x => x.Take(0).ToList());

            GenAssert.ShrinksTo(gen, new List<int>(), seed);
        });

        [Fact]
        public void WhenItIsEnumeratedOverOneIteration_ItShrinksToTheSmallestElement() => TestWithSeed(seed =>
        {
            var gen = GC.Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(x => x.Take(1).ToList());

            GenAssert.ShrinksTo(gen, new List<int> { 0 }, seed);
        });

        [Fact]
        public void WhenItIsEnumeratedOverTwoIteration_ItShrinksToTheSmallestElementTwice() => TestWithSeed(seed =>
        {
            var gen = GC.Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(x => x.Take(2).ToList());

            GenAssert.ShrinksTo(gen, new List<int> { 0, 0 }, seed);
        });

        [Fact]
        public void WhenItIsEnumeratedOverTwoIteration_AndThereIsADistinctPredicate_ItShrinksToTheSmallestAndNextSmallestElement() => TestWithSeed(seed =>
        {
            var gen = GC.Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(x => x.Take(2).ToList());

            GenAssert.ShrinksTo(gen, new List<int> { 1, 0 }, seed, xs => xs.Distinct().SequenceEqual(xs));
        });

        [Fact]
        public void WhenItIsEnumeratedAsASideEffect_ItShrinksToTheSmallestElementRepeating() => TestWithSeed(seed =>
        {
            var gen = GC.Gen
                .Int32()
                .Between(0, 10)
                .InfiniteOf()
                .Select(x =>
                {
                    x.Take(1).ToList();
                    return x;
                });

            GenAssert.ShrinksTo(gen, xs => Assert.Equal(Enumerable.Repeat(0, 10), xs.Take(10)), seed);
        });


        // TODO: Test the repeating behaviour
    }
}
