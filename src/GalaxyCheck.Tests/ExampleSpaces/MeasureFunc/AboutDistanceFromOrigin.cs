using FsCheck;
using FsCheck.Xunit;
using System;

namespace Tests.ExampleSpaces.MeasureFunc
{
    public class AboutDistanceFromOrigin
    {
        [Property]
        public FsCheck.Property WhenMinIsGreaterThanMax_ItThrows(int min, int max, int origin)
        {
            Action test = () =>
            {
            };

            return test.When(min > max);
        }
    }
}
