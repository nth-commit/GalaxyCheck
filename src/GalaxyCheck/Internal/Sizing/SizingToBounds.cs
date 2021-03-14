using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Internal.Sizing
{
    public delegate (int min, int max) SizeToBoundsFunc(Size size);

    public delegate SizeToBoundsFunc SizingToBoundsFunc(int min, int max, int origin);

    public static class SizingToBounds
    {
        public static SizingToBoundsFunc Exponential => (min, max, origin) =>
        {
            static Dictionary<Size, int> GetBoundsBySize(int from, int to)
            {
                var discreteBounds = GetIntervals(from, to).Select(interval => IntervalToBound(from, to, interval));
                return InterpolateBoundOverSizes(discreteBounds.ToImmutableList());
            }

            var leftIntervals = GetBoundsBySize(origin, min);
            var rightIntervals = GetBoundsBySize(origin, max);

            return (size) => (leftIntervals[size], rightIntervals[size]);
        };

        private static Func<int, int> Interpolate(int x1, int y1, int x2, int y2) => (x) =>
        {
            return (int)(y1 + (x - x1) * ((double)(y2 - y1) / (x2 - x1)));
        };

        private static int IntervalToBound(int from, int to, uint interval)
        {
            if (to > from)
            {
                return (int)(from + interval);
            }
            else
            {
                return (int)(from - interval);
            }
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

        private static IEnumerable<uint> GetIntervals(int from, int to)
        {
            const int MinimumIntervalCount = 6;

            var width = CalculateWidthSafe(from, to);
            var subMaxIntervals = WidthIntervals.Where(w => w < width).ToList();

            foreach (var interval in subMaxIntervals)
            {
                yield return interval;
            }

            yield return width;

            for (var i = subMaxIntervals.Count + 1; i < MinimumIntervalCount; i++)
            {
                yield return width;
            }
        }

        private static readonly ImmutableList<uint> WidthIntervals = BaseWidthIntervals 
            .Concat(ExponentialWidthIntervals)
            .Concat(AlmostMaxIntervals)
            .Concat(new[] { uint.MaxValue })
            .ToImmutableList();

        private static IEnumerable<uint> BaseWidthIntervals => new uint[] { 0, 1, 5 };

        private static IEnumerable<uint> ExponentialWidthIntervals => new[] { 1, 2, 4, 8 }.Select(exponent => (uint)Math.Pow(10, exponent));

        // These make the range from int.MinValue => int.MaxValue more interesting. The bound won't jump from < 0 to int.MaxValue.
        private static IEnumerable<uint> AlmostMaxIntervals => ImmutableList.Create(
            uint.MaxValue / 2,
            (uint.MaxValue / 4) * 3,
            (uint.MaxValue / 8) * 7);

        private static Dictionary<Size, int> InterpolateBoundOverSizes(ImmutableList<int> bounds)
        {
            var interpolate = Interpolate(0, 0, 100, bounds.Count - 1);

            return Enumerable.Range(0, 101).ToDictionary(
                size => new Size(size),
                size =>
                {
                    var interpolatedIndex = interpolate(size);
                    var boundIndex = size == 0 ? 0 : Math.Min(interpolatedIndex + 1, bounds.Count - 1);
                    return bounds[boundIndex];
                });
        }
    }
}
