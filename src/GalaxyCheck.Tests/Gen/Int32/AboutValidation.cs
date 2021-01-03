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
    }
}
