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

        private static uint CalculateWidthSafe(int from, int to)
        {
            var fromSign = Math.Sign(from);
            var toSign = Math.Sign(to);

            if (fromSign >= 0 && toSign >= 0)
            {
                return SafeNegate(to - from);
            }
            else if (toSign <= 0 && fromSign <= 0)
            {
                return SafeNegate(to - from);
            }
            else
            {
                var fromToZero = SafeNegate(from);
                var toToZero = SafeNegate(to);
                return fromToZero + toToZero;
            }
        }

        private static uint SafeNegate(int x)
        {
            return x == int.MinValue ? (uint)(int.MaxValue) + 1 : (uint)Math.Abs(x);
        }

        public static MeasureFunc<long> DistanceFromOrigin(long origin, long min, long max)
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

        private static ulong CalculateWidthSafe(long from, long to)
        {
            var fromSign = Math.Sign(from);
            var toSign = Math.Sign(to);

            if (fromSign >= 0 && toSign >= 0)
            {
                return SafeNegate(to - from);
            }
            else if (toSign <= 0 && fromSign <= 0)
            {
                return SafeNegate(to - from);
            }
            else
            {
                var fromToZero = SafeNegate(from);
                var toToZero = SafeNegate(to);
                return fromToZero + toToZero;
            }
        }

        private static ulong SafeNegate(long x)
        {
            return x == long.MinValue ? (ulong)(long.MaxValue) + 1 : (ulong)Math.Abs(x);
        }
    }
}
