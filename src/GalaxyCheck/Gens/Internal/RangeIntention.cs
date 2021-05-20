using System;

namespace GalaxyCheck.Gens.Internal
{
    internal record RangeIntention
    {
        private record _Unspecified : RangeIntention;

        private record _Exact(int Value) : RangeIntention;

        private record _Bounded(int? Minimum, int? Maximum) : RangeIntention;

        public T Match<T>(
            Func<T> onUnspecified,
            Func<int, T> onExact,
            Func<int?, int?, T> onBounded)
        {
            return this switch
            {
                _Unspecified _ => onUnspecified(),
                _Exact exact => onExact(exact.Value),
                _Bounded bounded => onBounded(bounded.Minimum, bounded.Maximum),
                _ => throw new NotSupportedException("Fatal: Unhandled case")
            };
        }

        public static RangeIntention Unspecified() => new _Unspecified();

        public static RangeIntention Exact(int value) => new _Exact(value);

        public static RangeIntention Bounded(int? minimum, int? maximum) => new _Bounded(minimum, maximum);
    }

    internal static class RangeIntentionExtensions
    {
        public static RangeIntention WithMinimum(this RangeIntention r, int minimum) => r.Match(
            onUnspecified: () => RangeIntention.Bounded(minimum, null),
            onExact: (_) => RangeIntention.Bounded(minimum, null),
            onBounded: (_, maximum) => RangeIntention.Bounded(minimum, maximum));

        public static RangeIntention WithMaximum(this RangeIntention r, int maximum) => r.Match(
            onUnspecified: () => RangeIntention.Bounded(null, maximum),
            onExact: (_) => RangeIntention.Bounded(null, maximum),
            onBounded: (minimum, _) => RangeIntention.Bounded(minimum, maximum));
    }
}
