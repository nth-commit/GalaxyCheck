using FsCheck;
using FsCheck.Xunit;
using System;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.ListGen
{
    [Properties(Arbitrary = new[] { typeof(ArbitraryGen) })]
    public class AboutValidation
    {
        [Property]
        public FsCheck.Property ItErrorsWhenOfLengthReceivesNegativeInt(GC.IGen<object> elementGen, int length)
        {
            Action test = () => TestWithSeed(seed => 
            {
                var gen = GC.Gen.List(elementGen).OfLength(length);

                var expectedMessage = "Error while running generator ListGen: 'length' cannot be negative";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(length < 0);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenOfMinimumLengthReceivesNegativeInt(GC.IGen<object> elementGen, int length)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.List(elementGen).OfMinimumLength(length);

                var expectedMessage = "Error while running generator ListGen: 'minLength' cannot be negative";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(length < 0);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenOfMaximumLengthReceivesNegativeInt(GC.IGen<object> elementGen, int length)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.List(elementGen).OfMaximumLength(length);

                var expectedMessage = "Error while running generator ListGen: 'maxLength' cannot be negative";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(length < 0);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenBetweenLengthsReceivesNegativeIntForFirstBound(GC.IGen<object> elementGen, int x, int y)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.List(elementGen).BetweenLengths(x, y);

                var expectedMessage = "Error while running generator ListGen: 'minLength' cannot be negative";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(x < 0 && x < y);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenBetweenLengthsReceivesNegativeIntForSecondBound(GC.IGen<object> elementGen, int x, int y)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.List(elementGen).BetweenLengths(x, y);

                var expectedMessage = "Error while running generator ListGen: 'minLength' cannot be negative";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(y < 0 && y < x);
        }

        [Property]
        public FsCheck.Property ItErrorsAboutMinimumLengthWhenBetweenLengthsReceivesNegativeIntForBothBounds(GC.IGen<object> elementGen, int x, int y)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.List(elementGen).BetweenLengths(x, y);

                var expectedMessage = "Error while running generator ListGen: 'minLength' cannot be negative";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(x < 0 && y < 0);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenMinimumLengthIsGreaterThanMaximumLength(GC.IGen<object> elementGen, int minLength, int maxLength)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var genMinBeforeMax = GC.Gen.List(elementGen).OfMinimumLength(minLength).OfMaximumLength(maxLength);
                var genMaxBeforeMin = GC.Gen.List(elementGen).OfMaximumLength(maxLength).OfMinimumLength(minLength);
                
                var expectedMessage = "Error while running generator ListGen: 'minLength' cannot be greater than 'maxLength'";
                GenAssert.Errors(genMinBeforeMax, expectedMessage, seed);
                GenAssert.Errors(genMaxBeforeMin, expectedMessage, seed);
            });

            return test.When(minLength > maxLength && maxLength >= 0);
        }
    }
}
