using System;

namespace GalaxyCheck.ExampleSpaces
{
    public delegate decimal MeasureFunc<T>(T value);

    public static class MeasureFunc
    {
        public static MeasureFunc<T> Unmeasured<T>() => (_) => 0;

        // TODO: Implement & test
        public static MeasureFunc<int> DistanceFromOrigin(int origin, int min, int max)
        {
            if (min > max)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(min),
                    "'min' cannot be greater than 'max'");
            }
            
            if (origin < min || origin > max)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(origin),
                    "'origin' must be between 'min' and 'max'");
            }

            return (value) =>
            {
                if (value < min)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "'value' cannot be less than 'min'");
                }

                if (value > max)
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(value),
                        "'value' cannot be greater than 'max'");
                }

                if (value == origin) return 0;
                if (value == min || value == max) return 100;

                var valueAbsoluteDistance = CalculateWidthSafe(origin, value);
                if (value < origin)
                {
                    var leftAbsoluteDistance = CalculateWidthSafe(origin, min);
                    return (decimal)valueAbsoluteDistance / leftAbsoluteDistance * 100;
                }
                else
                {
                    var rightAbsoluteDistance = CalculateWidthSafe(origin, max);
                    return (decimal)valueAbsoluteDistance / rightAbsoluteDistance * 100;
                }
            };
        }

        private static int CalculateWidthSafe(int from, int to)
        {
            var unsignedWidth = to - from;

            if (unsignedWidth == int.MinValue)
            {
                // int.MinValue is -2147483648, which when negated, exceeds int.MaxValue (2147483647).
                return int.MaxValue;
            }

            return Math.Abs(unsignedWidth);
        }
    }
}
