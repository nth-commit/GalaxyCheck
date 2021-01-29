using FsCheck;
using FsCheck.Xunit;
using System;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.ListGen
{
    [Properties(MaxTest = 10)]
    public class AboutShrinking
    {
        [Property(EndSize = 50)]
        public FsCheck.Property ItShrinksLengthToTheMinimum(int minLength, int maxLength, GC.Gen.Bias bias, object elementValue)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Constant(elementValue)
                    .ListOf()
                    .OfMinimumLength(minLength)
                    .OfMaximumLength(maxLength)
                    .WithLengthBias(bias);

                var expectedMinimal = Enumerable.Repeat(elementValue, minLength).ToImmutableList();
                GenAssert.ShrinksTo(gen, expectedMinimal, seed);
            });

            return test.When(minLength >= 0 && maxLength >= minLength);
        }

        [Property(EndSize = 50)]
        public FsCheck.Property ItShrinksLengthToTheLocalMinimum(int localMinLength, GC.Gen.Bias bias, object elementValue)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen
                    .Constant(elementValue)
                    .ListOf()
                    .WithLengthBias(bias);

                Func<ImmutableList<object>, bool> pred = l => l.Count >= localMinLength;
                var expectedMinimal = Enumerable.Repeat(elementValue, localMinLength).ToImmutableList();
                GenAssert.ShrinksTo(gen, expectedMinimal, seed, pred);
            });

            return test.When(localMinLength >= 0 && localMinLength <= 20);
        }

        [Fact()]
        public void ItShrinksToTheSingleSmallestElement() => TestWithSeed(seed =>
        {
            var gen = GC.Gen
                .Int32()
                .Between(0, 10)
                .ListOf();

            var expectedShrink = new[] { 5 };
            GenAssert.ShrinksTo(
                gen,
                expectedShrink.ToImmutableList(),
                seed,
                (xs) => xs.Any(x => x >= 5));
        });

        [Fact()]
        public void ItShrinksToACombinationOfTheTwoSmallestElements() => TestWithSeed(seed =>
        {
            var elementGen = GC.Gen.Int32().Between(0, 10);
            var listGen = elementGen.ListOf();

            var expectedShrink = new[] { 6, 5 };
            GenAssert.ShrinksTo(listGen, expectedShrink.ToImmutableList(), seed, (xs) => xs.Sum() >= 11);
        });
    }
}
