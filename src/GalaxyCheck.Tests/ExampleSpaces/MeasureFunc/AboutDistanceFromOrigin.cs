using FsCheck;
using FsCheck.Xunit;
using System;

namespace GalaxyCheck.Tests.ExampleSpaces.MeasureFunc
{
    public class AboutDistanceFromOrigin
    {
        [Property]
        public Property WhenMinIsGreaterThanMax_ItThrows(int min, int max, int origin)
        {
            Action test = () =>
            {
            };

            return test.When(min > max);
        }
    }
}
