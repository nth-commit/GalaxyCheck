using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public delegate IEnumerable<T> ShrinkFunc<T>(T value);

    public static class ShrinkFunc
    {
        public static ShrinkFunc<T> None<T>() => (_) => Enumerable.Empty<T>();

        public static ShrinkFunc<int> Towards(int target) => (value) => value == target
            ? Enumerable.Empty<int>()
            : Halves(value - target).Select(difference => value - difference);

        private static IEnumerable<int> Halves(int value) =>
            Unfold(Math.Abs(value), x => x > 0, x => x / 2).Select(x => Math.Sign(value) == -1 ? -x : x);

        private static IEnumerable<T> Unfold<T>(
            T seed,
            Func<T, bool> continuationCondition,
            Func<T, T> generateNext)
        {
            var current = seed;
            do
            {
                yield return current;
                current = generateNext(current);
            } while (continuationCondition(current));
        }
    }
}
