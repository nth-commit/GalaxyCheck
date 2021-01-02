using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    /// <summary>
    /// A function which "shrinks" a value. Generates a sample of smaller incantations of the original value. An
    /// implementation needn't generate all possible smaller values, as the function is generally applied recursively.
    /// </summary>
    /// <typeparam name="T">The type of the value to be shrunk.</typeparam>
    /// <param name="value">The value which should be shrunk.</param>
    /// <returns>A sample of smaller values, or an empty enumerable if the value cannot shrink.</returns>
    public delegate IEnumerable<T> ShrinkFunc<T>(T value);

    public static class ShrinkFunc
    {
        /// <summary>
        /// Creates a no-op shrink function for `T`, which always returns an empty enumerable (indicating that any
        /// given value will not shrink).
        /// </summary>
        /// <typeparam name="T">The type of the value which should not be shrunk.</typeparam>
        /// <returns>A no-op shrink function for any type.</returns>
        public static ShrinkFunc<T> None<T>() => (_) => Enumerable.Empty<T>();

        /// <summary>
        /// Creates a shrink function that generates the target (i.e. the smallest possible value), edging towards
        /// the given value. For example, if the target is 0, and the given value is 100, it will generate the
        /// shrinks [0, 50, 75, 88, 94, 97, 99]. When applied recursively it allows a binary search to find the
        /// smallest value which meets certain conditions.
        /// </summary>
        /// <param name="target">The smallest possible integer that should be shrunk to.</param>
        /// <returns>A shrink function for integers.</returns>
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
