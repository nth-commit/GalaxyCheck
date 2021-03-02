using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Core;
using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Infinite
{
    [Properties(
        MaxTest = 10,
        Arbitrary = new [] { typeof(ArbitraryIterations), typeof(ArbitrarySize), typeof(ArbitraryIterationLimit) })]
    public class AboutIterationLimits
    {
        public record IterationLimit(int Value);

        public class ArbitraryIterationLimit
        {
            private static readonly FSharpFunc<IterationLimit, IEnumerable<IterationLimit>> IterationLimitShrinker =
                FuncConvert.FromFunc<IterationLimit, IEnumerable<IterationLimit>>(iterationLimit =>
                    Arb.Shrink(iterationLimit.Value)
                        .Where(x => x >= 1)
                        .Select(x => new IterationLimit(x)));

            public static Arbitrary<IterationLimit> IterationLimit() => FsCheck.Gen.Choose(1, 100).Select(x => new IterationLimit(x)).ToArbitrary(IterationLimitShrinker);
        }

        [Property]
        public void TheDefaultLimitIs1000(Size size)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().InfiniteOf();

                var enumerable = gen.SampleOne(seed: seed, size: size.Value);

                AssertLimit(enumerable, 1_000);
            });
        }

        [Property]
        public void AGeneratedEnumerableWithoutALimitNeverThrows(Size size)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .InfiniteOf()
                    .WithoutIterationLimit();

                var enumerable = gen.SampleOne(seed: seed, size: size.Value);

                enumerable.Take(1_001).ToList();
            });
        }

        [Property]
        public void AGeneratedEnumerableHasTheGivenLimit(Size size, IterationLimit limit)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .InfiniteOf()
                    .WithIterationLimit(limit.Value);

                var enumerable = gen.SampleOne(seed: seed, size: size.Value);

                AssertLimit(enumerable, limit.Value);
            });
        }

        [Fact]
        public void AShrinkOfAGeneratedEnumerableWithoutALimitNeverThrows()
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .InfiniteOf()
                    .WithoutIterationLimit()
                    .Select(EnsureSourceCanShrink);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed);
                var shrunkEnumerable = exampleSpace.Subspace.First().Current.Value;

                shrunkEnumerable.Take(1_001).ToList();
            });
        }

        [Property]
        public FsCheck.Property AShrinkOfAGeneratedEnumerableHasTheGivenLimit(IterationLimit limit)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .InfiniteOf()
                    .WithIterationLimit(limit.Value)
                    .Select(EnsureSourceCanShrink);

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed);
                var shrunkEnumerable = exampleSpace.Subspace.First().Current.Value;

                AssertLimit(shrunkEnumerable, limit.Value);
            });

            return test.When(limit.Value > 1);
        }

        private static void AssertLimit<T>(IEnumerable<T> enumerable, int expectedLimit)
        {
            // It doesn't thrown when the limit is hit
            enumerable.Take(expectedLimit).ToList();

            // It throws when it exceeds the limit
            Action throwing = () => enumerable.Take(expectedLimit + 1).ToList();
            var exception = Assert.Throws<GC.Exceptions.GenLimitExceededException>(throwing);
            Assert.Equal("Infinite enumerable exceeded iteration limit. This is a built-in safety mechanism to prevent hanging tests. Use IInfiniteGen.WithIterationLimit or IInfiniteGen.WithoutIterationLimit to modify this limit.", exception.Message);
        }

        private static IEnumerable<T> EnsureSourceCanShrink<T>(IEnumerable<T> source)
        {
            // Ensure the enumerable has a shrink by enumerating at least two elements. Then it will be able to at
            // least shrink to the enumerable that repeats a single element.
            source.Take(2).ToList();
            return source;
        }
    }
}
