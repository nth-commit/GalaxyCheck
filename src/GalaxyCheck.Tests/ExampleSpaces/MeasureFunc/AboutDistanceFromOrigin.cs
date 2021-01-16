using FsCheck;
using FsCheck.Xunit;
using System;
using Xunit;
using ES = GalaxyCheck.Internal.ExampleSpaces;


namespace Tests.ExampleSpaces.MeasureFunc
{
    public class AboutDistanceFromOrigin
    {
        [Property]
        public FsCheck.Property IfMinIsGreaterThanMax_ItThrows(int origin, int min, int max)
        {
            Action test = () =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    ES.MeasureFunc.DistanceFromOrigin(origin, min, max);
                });
            };

            return test.When(min > max);
        }

        [Property]
        public FsCheck.Property IfOriginIsNotBetweenMinAndMax_ItThrows(int origin, int min, int max)
        {
            Action test = () =>
            {
                Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    ES.MeasureFunc.DistanceFromOrigin(origin, min, max);
                });
            };

            return test.When(min <= max && (origin < min || origin > max));
        }

        [Property]
        public FsCheck.Property IfValueIsLessThanMin_ItThrows(int origin, int width, int value)
        {
            var min = origin - width;
            var max = origin + width;

            Action test = () =>
            {
                var measure = ES.MeasureFunc.DistanceFromOrigin(origin, min, max);

                Assert.Throws<ArgumentOutOfRangeException>(() => measure(value));
            };

            return test.When(width >= 0 && value < min);
        }

        [Property]
        public FsCheck.Property IfValueIsGreaterThanMax_ItThrows(int origin, int width, int value)
        {
            var min = origin - width;
            var max = origin + width;

            Action test = () =>
            {
                var measure = ES.MeasureFunc.DistanceFromOrigin(origin, min, max);

                Assert.Throws<ArgumentOutOfRangeException>(() => measure(value));
            };

            return test.When(width >= 0 && value > max);
        }

        [Property]
        public FsCheck.Property IfValueEqualsOrigin_ItReturnsZero(int originAndValue, int min, int max)
        {
            Action test = () =>
            {
                var measure = ES.MeasureFunc.DistanceFromOrigin(originAndValue, min, max);

                var distance = measure(originAndValue);

                Assert.Equal(0, distance);
            };

            return test.When(originAndValue >= min && originAndValue <= max);
        }

        [Property]
        public FsCheck.Property IfValueEqualsMin_ItReturns100(int origin, int minAndValue, int max)
        {
            Action test = () =>
            {
                var measure = ES.MeasureFunc.DistanceFromOrigin(origin, minAndValue, max);

                var distance = measure(minAndValue);

                Assert.Equal(100, distance);
            };

            return test.When(origin >= minAndValue && origin <= max && origin != minAndValue);
        }

        [Property]
        public FsCheck.Property IfValueEqualsMax_ItReturns100(int origin, int min, int maxAndValue)
        {
            Action test = () =>
            {
                var measure = ES.MeasureFunc.DistanceFromOrigin(origin, min, maxAndValue);

                var distance = measure(maxAndValue);

                Assert.Equal(100, distance);
            };

            return test.When(origin >= min && origin <= maxAndValue && origin != maxAndValue);
        }

        [Property]
        public FsCheck.Property ItReflectsAroundOrigin(int origin, int width, int valueWidth)
        {
            var min = origin - width;
            var max = origin + width;

            var leftValue = origin - valueWidth;
            var rightValue = origin + valueWidth;

            Action test = () =>
            {
                var measure = ES.MeasureFunc.DistanceFromOrigin(origin, min, max);

                var distance0 = measure(leftValue);
                var distance1 = measure(rightValue);

                Assert.Equal(distance0, distance1);
            };

            return test.When(width >= 0 && valueWidth >= 0 && valueWidth <= width);
        }

        [Theory]
        [InlineData(0, -100, 50, 25, 50)]
        [InlineData(50, -50, 100, 75, 50)]
        [InlineData(0, 0, 4, 1, 25)]
        [InlineData(0, 0, 3, 1, 33.33333)]
        public void ExamplesForValuesBetweenOriginAndMax(int origin, int min, int max, int value, decimal expectedDistance)
        {
            var measure = ES.MeasureFunc.DistanceFromOrigin(origin, min, max);

            var distance = measure(value);

            Assert.Equal(expectedDistance, distance, 5);
        }
    }
}
