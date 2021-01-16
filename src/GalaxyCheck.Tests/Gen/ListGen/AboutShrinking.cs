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
            },
            0);

            return test.When(localMinLength >= 0 && localMinLength <= 20);
        }

        [Property(EndSize = 50)]
        public FsCheck.Property ItShrinksToTheSingleSmallestElement(int max, int minCounterexampleElement)
        {
            Action test = () => TestWithSeed(seed =>
            {
                // TODO: Bias needs to be none so that it has a high chance of picking a number that hits the minimum
                // counterexample. Need to improve the sizing algorithm so this is not required.
                var gen = GC.Gen
                    .Int32()
                    .Between(0, max)
                    .WithBias(GC.Gen.Bias.None)
                    .ListOf();

                var expectedShrink = ImmutableList.Create(minCounterexampleElement);
                GenAssert.ShrinksTo(
                    gen,
                    expectedShrink,
                    seed,
                    (xs) => xs.Any(x => x >= minCounterexampleElement));
            });

            return test.When(minCounterexampleElement > 0 && max > minCounterexampleElement);
        }
    }
}
