﻿using FsCheck;
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

        [Property]
        public void AShrinkOfAGeneratedEnumerableWithoutALimitNeverThrows(Size size)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .InfiniteOf()
                    .WithoutIterationLimit()
                    .Select(source =>
                    {
                        // Ensure the enumerable has a shrink by enumerating at least one element.
                        source.Take(1).ToList();
                        return source;
                    });

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size.Value);
                var shrunkEnumerable = exampleSpace.Subspace.First().Current.Value;

                shrunkEnumerable.Take(1_001).ToList();
            });
        }

        [Property]
        public void AShrinkOfAGeneratedEnumerableHasTheGivenLimit(Size size, IterationLimit limit)
        {
            TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Int32()
                    .InfiniteOf()
                    .WithIterationLimit(limit.Value)
                    .Select(source =>
                    {
                        // Ensure the enumerable has a shrink by enumerating at least one element.
                        source.Take(1).ToList();
                        return source;
                    });

                var exampleSpace = gen.Advanced.SampleOneExampleSpace(seed: seed, size: size.Value);
                var shrunkEnumerable = exampleSpace.Subspace.First().Current.Value;

                AssertLimit(shrunkEnumerable, limit.Value);
            });
        }

        private static void AssertLimit<T>(IEnumerable<T> enumerable, int expectedLimit)
        {
            // It doesn't thrown when hit
            enumerable.Take(expectedLimit).ToList();

            // It throws when exceeded
            Action throwing = () => enumerable.Take(expectedLimit + 1).ToList();
            var exception = Assert.Throws<GC.Gens.InfiniteGenLimitExceededException>(throwing);
            Assert.Equal("Infinite enumerable exceeded iteration limit. This is a built-in safety mechanism to prevent hanging tests. Use IInfiniteGen{T}.WithIterationLimit(int) or IInfiniteGen{T}.WithoutIterationLimit to modify this limit.", exception.Message);
        }
    }
}