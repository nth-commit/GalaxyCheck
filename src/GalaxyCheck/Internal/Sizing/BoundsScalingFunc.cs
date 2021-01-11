using System;

namespace GalaxyCheck.Internal.Sizing
{
    public delegate (int min, int max) BoundsScalingFunc(Size size);

    public delegate BoundsScalingFunc BoundsScalingFactoryFunc(int min, int max, int origin);

    public static class BoundsScalingFactoryFuncs
    {
        public static BoundsScalingFactoryFunc Unscaled => (min, max, origin) => (size) =>
        {
            return (min, max);
        };

        public static BoundsScalingFactoryFunc ScaledLinearly => (min, max, origin) => (size) =>
        {
            if (size.Value == Size.MinValue.Value) return (origin, origin);
            if (size.Value == Size.MaxValue.Value) return (min, max);

            return (ScaleLinear(min, origin, size), ScaleLinear(origin, max, size));
        };

        public static BoundsScalingFactoryFunc ScaledExponentially => (min, max, origin) => (size) =>
        {
            if (size.Value == Size.MinValue.Value) return (origin, origin);
            if (size.Value == Size.MaxValue.Value) return (min, max);

            return (ScaleExponential(min, origin, size), ScaleExponential(origin, max, size));
        };

        private static int ScaleLinear(int from, int to, Size size)
        {
            var width = CalculateWidthSafe(from, to);
            var multiplier = (double)size.Value / Size.MaxValue.Value;
            var scaledWidth = (int)Math.Round(width * multiplier);

            return from < 0
                ? to > from ? to - scaledWidth : to + scaledWidth
                : to > from ? from + scaledWidth : from - scaledWidth;
        }

        private static int ScaleExponential(int from, int to, Size size)
        {
            var width = CalculateWidthSafe(from, to);
            var exponent = ((double)size.Value / Size.MaxValue.Value) - 1;
            var multiplier = Math.Pow(Size.MaxValue.Value, exponent);
            var scaledWidth = (int)Math.Round(width * multiplier);

            return from < 0
                ? to > from ? to - scaledWidth : to + scaledWidth
                : to > from ? from + scaledWidth : from - scaledWidth;
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
