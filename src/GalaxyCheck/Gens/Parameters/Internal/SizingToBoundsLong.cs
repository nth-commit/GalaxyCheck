using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace GalaxyCheck.Gens.Parameters.Internal
{
    internal delegate (long min, long max) SizeToBoundsLongFunc(Size size);

    internal delegate SizeToBoundsLongFunc SizingToBoundsLongFunc(long min, long max, long origin);

    internal static class SizingToBoundsLong
    {
        public static SizingToBoundsLongFunc Exponential => (min, max, origin) =>
        {
            static IReadOnlyDictionary<int, long> GetBoundsBySize(long from, long to)
            {
                var discreteBounds = GetIntervals(from, to).Select(interval => IntervalToBound(from, to, interval));
                return InterpolateBoundOverSizes(discreteBounds.ToList());
            }

            var leftIntervals = GetBoundsBySize(origin, min);
            var rightIntervals = GetBoundsBySize(origin, max);

            return (size) => (leftIntervals[size.Value], rightIntervals[size.Value]);
        };

        private static Func<int, int> Interpolate(int x1, int y1, int x2, int y2) => (x) =>
        {
            return (int)(y1 + (x - x1) * ((double)(y2 - y1) / (x2 - x1)));
        };

        private static long IntervalToBound(long from, long to, ulong interval)
        {
            if (to > from)
            {
                return (long)((ulong)from + interval);
            }
            else
            {
                return (long)((ulong)from - interval);
            }
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

        private static IEnumerable<ulong> GetIntervals(long from, long to)
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

        private static readonly ImmutableList<ulong> WidthIntervals = BaseWidthIntervals 
            .Concat(ExponentialWidthIntervals)
            .Concat(AlmostMaxIntervals)
            .Concat(new[] { ulong.MaxValue })
            .ToImmutableList();

        private static IEnumerable<ulong> BaseWidthIntervals => new ulong[] { 0, 1, 5 };

        private static IEnumerable<ulong> ExponentialWidthIntervals => new[] { 1, 2, 4, 8 }.Select(exponent => (ulong)Math.Pow(10, exponent));

        // These make the range from long.MinValue => long.MaxValue more interesting. The bound won't jump from < 0 to long.MaxValue.
        private static IEnumerable<ulong> AlmostMaxIntervals => ImmutableList.Create(
            ulong.MaxValue / 2,
            (ulong.MaxValue / 4) * 3,
            (ulong.MaxValue / 8) * 7);

        private static IReadOnlyDictionary<int, long> InterpolateBoundOverSizes(IReadOnlyList<long> bounds)
        {
            var interpolate = Interpolate(0, 0, 100, bounds.Count - 1);

            return new LazyRangedDictionary<long>(0, 100, (size) =>
            {
                var interpolatedIndex = interpolate(size);
                var boundIndex = size == 0 ? 0 : Math.Min(interpolatedIndex + 1, bounds.Count - 1);
                return bounds[boundIndex];
            });
        }

        private class LazyRangedDictionary<Value> : IReadOnlyDictionary<int, Value>
        {
            private readonly int _start;
            private readonly int _end;
            private readonly Func<int, Value> _produceValue;
            private readonly int _count;

            private readonly Dictionary<int, Value> _internalValues = new();

            public LazyRangedDictionary(int start, int end, Func<int, Value> produceValue)
            {
                _start = start;
                _end = end;
                _produceValue = produceValue;
                _count = end - start + 1;
            }

            public Value this[int key]
            {
                get
                {
                    if (_internalValues.TryGetValue(key, out var value) == false)
                    {
                        value = _produceValue(key);
                        _internalValues[key] = value;
                    }

                    return value;
                }
            }

            public IEnumerable<int> Keys => Enumerable.Range(_start, _count);

            public IEnumerable<Value> Values => Keys.Select(k => this[k]);

            public int Count => _count;

            public bool ContainsKey(int key) => key >= _start && key <= _end;

            public IEnumerator<KeyValuePair<int, Value>> GetEnumerator() => Keys.Select(k => new KeyValuePair<int, Value>(k, this[k])).GetEnumerator();

            public bool TryGetValue(int key, [MaybeNullWhen(false)] out Value value)
            {
                if (ContainsKey(key))
                {
                    value = this[key];
                    return true;
                }
                else
                {
                    value = default;
                    return false;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
