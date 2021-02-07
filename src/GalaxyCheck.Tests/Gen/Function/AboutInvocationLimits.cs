using FsCheck;
using FsCheck.Xunit;
using Microsoft.FSharp.Core;
using System.Linq;
using System.Collections.Generic;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;
using System;
using Xunit;

namespace Tests.Gen.Function
{
    public class AboutInvocationLimits
    {
        public record InvocationLimit(int Value);

        public class ArbitraryInvocationLimit
        {
            private static readonly FSharpFunc<InvocationLimit, IEnumerable<InvocationLimit>> InvocationLimitShrinker =
                FuncConvert.FromFunc<InvocationLimit, IEnumerable<InvocationLimit>>(invocationLimit =>
                    Arb.Shrink(invocationLimit.Value)
                        .Where(x => x >= 1)
                        .Select(x => new InvocationLimit(x)));

            public static Arbitrary<InvocationLimit> InvocationLimit() => FsCheck.Gen.Choose(1, 100).Select(x => new InvocationLimit(x)).ToArbitrary(InvocationLimitShrinker);
        }

        [Properties( MaxTest = 10, Arbitrary = new[] { typeof(ArbitrarySize), typeof(ArbitraryInvocationLimit) })]
        public class NullaryFunction
        {
            [Property]
            public void AGeneratedFunctionWithoutALimitNeverThrows(Size size)
            {
                ItIsLimitless(GC.Gen.Function(GC.Gen.Int32()).WithoutInvocationLimit(), size.Value);
            }

            [Property]
            public void AGeneratedFunctionHasTheGivenLimit(Size size, InvocationLimit limit)
            {
                ItHasGivenLimit(GC.Gen.Function(GC.Gen.Int32()).WithInvocationLimit(limit.Value), size.Value, limit.Value);
            }
        }

        [Properties(MaxTest = 10, Arbitrary = new[] { typeof(ArbitrarySize), typeof(ArbitraryInvocationLimit) })]
        public class UnaryFunction
        {
            [Property]
            public void AGeneratedFunctionWithoutALimitNeverThrows(Size size)
            {
                var gen = GC.Gen.Function<int, int>(GC.Gen.Int32())
                    .WithoutInvocationLimit()
                    .Select<Func<int, int>, Func<int>>(f => () => f(0));

                ItIsLimitless(gen, size.Value);
            }

            [Property]
            public void AGeneratedFunctionHasTheGivenLimit(Size size, InvocationLimit limit)
            {
                var gen = GC.Gen.Function<int, int>(GC.Gen.Int32())
                    .WithInvocationLimit(limit.Value)
                    .Select<Func<int, int>, Func<int>>(f => () => f(0));

                ItHasGivenLimit(gen, size.Value, limit.Value);
            }
        }

        [Properties(MaxTest = 10, Arbitrary = new[] { typeof(ArbitrarySize), typeof(ArbitraryInvocationLimit) })]
        public class BinaryFunction
        {
            [Property]
            public void AGeneratedFunctionWithoutALimitNeverThrows(Size size)
            {
                var gen = GC.Gen.Function<int, int, int>(GC.Gen.Int32())
                    .WithoutInvocationLimit()
                    .Select<Func<int, int, int>, Func<int>>(f => () => f(0, 1));

                ItIsLimitless(gen, size.Value);
            }

            [Property]
            public void AGeneratedFunctionHasTheGivenLimit(Size size, InvocationLimit limit)
            {
                var gen = GC.Gen.Function<int, int, int>(GC.Gen.Int32())
                    .WithInvocationLimit(limit.Value)
                    .Select<Func<int, int, int>, Func<int>>(f => () => f(0, 1));

                ItHasGivenLimit(gen, size.Value, limit.Value);
            }
        }

        [Properties(MaxTest = 10, Arbitrary = new[] { typeof(ArbitrarySize), typeof(ArbitraryInvocationLimit) })]
        public class TernaryFunction
        {
            [Property]
            public void AGeneratedFunctionWithoutALimitNeverThrows(Size size)
            {
                var gen = GC.Gen.Function<int, int, int, int>(GC.Gen.Int32())
                    .WithoutInvocationLimit()
                    .Select<Func<int, int, int, int>, Func<int>>(f => () => f(0, 1, 2));

                ItIsLimitless(gen, size.Value);
            }

            [Property]
            public void AGeneratedFunctionHasTheGivenLimit(Size size, InvocationLimit limit)
            {
                var gen = GC.Gen.Function<int, int, int, int>(GC.Gen.Int32())
                    .WithInvocationLimit(limit.Value)
                    .Select<Func<int, int, int, int>, Func<int>>(f => () => f(0, 1, 2));

                ItHasGivenLimit(gen, size.Value, limit.Value);
            }
        }

        [Properties(MaxTest = 10, Arbitrary = new[] { typeof(ArbitrarySize), typeof(ArbitraryInvocationLimit) })]
        public class QuarternaryFunction
        {
            [Property]
            public void AGeneratedFunctionWithoutALimitNeverThrows(Size size)
            {
                var gen = GC.Gen.Function<int, int, int, int, int>(GC.Gen.Int32())
                    .WithoutInvocationLimit()
                    .Select<Func<int, int, int, int, int>, Func<int>>(f => () => f(0, 1, 2, 3));

                ItIsLimitless(gen, size.Value);
            }

            [Property]
            public void AGeneratedFunctionHasTheGivenLimit(Size size, InvocationLimit limit)
            {
                var gen = GC.Gen.Function<int, int, int, int, int>(GC.Gen.Int32())
                    .WithInvocationLimit(limit.Value)
                    .Select<Func<int, int, int, int, int>, Func<int>>(f => () => f(0, 1, 2, 3));

                ItHasGivenLimit(gen, size.Value, limit.Value);
            }
        }

        private static void ItIsLimitless<TResult>(IGen<Func<TResult>> gen, int size)
        {
            TestWithSeed(seed =>
            {
                var func = gen.SampleOne(seed: seed, size: size);

                Enumerable.Range(0, 1_001).Select(_ => func()).ToList();
            });
        }

        private static void ItHasGivenLimit<TResult>(IGen<Func<TResult>> gen, int size, int limit)
        {
            TestWithSeed(seed =>
            {
                var func = gen.SampleOne(seed: seed, size: size);

                AssertLimit(func, limit);
            });
        }

        private static void AssertLimit<T>(Func<T> func, int expectedLimit)
        {
            // It doesn't thrown when the limit is hit
            Enumerable.Range(0, expectedLimit).Select(_ => func()).ToList();

            // It throws when exceeded
            Action throwing = () => Enumerable.Range(0, expectedLimit + 1).Select(_ => func()).ToList();
            var exception = Assert.Throws<GC.Exceptions.GenLimitExceededException>(throwing);
            Assert.Equal("Function exceeded invocation limit. This is a built-in safety mechanism to prevent hanging tests. Use IFunctionGen.WithInvocationLimit or IFunctionGen.WithoutInvocationLimit to modify this limit.", exception.Message);
        }
    }
}
