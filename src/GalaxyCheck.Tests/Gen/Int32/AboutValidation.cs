using FsCheck;
using FsCheck.Xunit;
using System;
using G = GalaxyCheck.Gen;
using static GalaxyCheck.Tests.TestUtils;

namespace GalaxyCheck.Tests.Gen.Int32
{
    public class AboutValidation
    {
        [Property]
        public Property ItErrorsWhenMinIsGreaterThanMax(int min, int max)
        {
            Action test = () => TestWithSeed(seed => 
            {
                var gen = G.Int32().GreaterThanEqual(max).LessThanEqual(min);

                var expectedMessage = "Error while running generator Int32GenBuilder: 'min' cannot be greater than 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(min < max);
        }

        [Property]
        public Property ItErrorsWhenMinIsGreaterThanOrigin(int min, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().GreaterThanEqual(min).ShrinkTowards(origin);

                var expectedMessage = "Error while running generator Int32GenBuilder: 'origin' must be between 'min' and 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(min > origin);
        }

        [Property]
        public Property ItErrorsWhenMaxIsLessThanOrigin(int max, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().LessThanEqual(max).ShrinkTowards(origin);

                var expectedMessage = "Error while running generator Int32GenBuilder: 'origin' must be between 'min' and 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(max < origin);
        }

        [Property]
        public Property ItErrorsWhenOriginIsNotBetweenRange(int x, int y, int origin)
        {
            Action test = () => TestWithSeed(seed =>
            {
                var gen = G.Int32().Between(x, y).ShrinkTowards(origin);

                var expectedMessage = "Error while running generator Int32GenBuilder: 'origin' must be between 'min' and 'max'";
                GenAssert.Errors(gen, expectedMessage, seed);
            });

            return test.When(x <= y && (origin < x || origin > y));
        }
    }
}
