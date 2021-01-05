﻿using GalaxyCheck.Abstractions;
using System;

namespace GalaxyCheck.Sizing
{
    public delegate (int min, int max) BoundsScalingFunc(ISize size);

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

        private static int ScaleLinear(int from, int to, ISize size)
        {
            var width = CalculateWidthSafe(from, to);
            var multiplier = (decimal)size.Value / 100;
            var scaledWidth = (int)Math.Round(width * multiplier);
            return to > from ? from + scaledWidth : from - scaledWidth;
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
