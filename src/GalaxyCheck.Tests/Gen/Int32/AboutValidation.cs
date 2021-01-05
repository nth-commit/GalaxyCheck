using FsCheck;
using FsCheck.Xunit;
using System;
using GC = GalaxyCheck;
using static Tests.TestUtils;

namespace Tests.Gen.Int32
{
    public class AboutValidation
    {
        [Property]
        public FsCheck.Property ItErrorsWhenMinIsGreaterThanMax(int min, int max)
        {
            Action test = () => TestWithSeed(seed => 
            {
                var gen = GC.Gen.Int32().GreaterThanEqual(max).LessThanEqual(min);

                var expectedMessage = "Error while running generator Int32Gen: 'min' cannot be greater than 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(min < max);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenMinIsGreaterThanOrigin(int min, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().GreaterThanEqual(min).ShrinkTowards(origin);

                var expectedMessage = "Error while running generator Int32Gen: 'origin' must be between 'min' and 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(min > origin);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenMaxIsLessThanOrigin(int max, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().LessThanEqual(max).ShrinkTowards(origin);

                var expectedMessage = "Error while running generator Int32Gen: 'origin' must be between 'min' and 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(max < origin);
        }

        [Property]
        public FsCheck.Property ItErrorsWhenOriginIsNotBetweenRange(int x, int y, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = GC.Gen.Int32().Between(x, y).ShrinkTowards(origin);

                var expectedMessage = "Error while running generator Int32Gen: 'origin' must be between 'min' and 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(x <= y && (origin < x || origin > y));
        }
    }
}
