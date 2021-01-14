﻿using GalaxyCheck;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Internal.ExampleSpaces
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

        /// <summary>
        /// Creates a shrink function that generates all the combinations (of n choose k) of the given enumerable.
        /// However, if k >= n, it returns an empty enumerable.
        /// </summary>
        /// <param name="k">The number of elements to choose for a combination.</param>
        /// <returns>A shrink function for enumerables.</returns>
        public static ShrinkFunc<IEnumerable<T>> OtherCombinations<T>(int k)
        {
            static IEnumerable<IEnumerable<T>> TailCombinations(int k, List<T> list)
            {
                if (list.Any() == false)
                {
                    return Enumerable.Empty<IEnumerable<T>>();
                }

                var head = list[0];
                var tail = list.Skip(1).ToList();
                return AllCombinations(k - 1, tail).Select(tail => Enumerable.Concat(new[] { head }, tail));
            }

            static IEnumerable<IEnumerable<T>> AllCombinations(int k, List<T> list)
            {
                if (k == 1)
                {
                    foreach (var element in list)
                    {
                        yield return new[] { element };
                    }
                }
                else
                {
                    var n = list.Count;

                    for (int i = 0; i < n; i++)
                    {
                        var subList = list.Skip(i).ToList();
                        foreach (var combination in TailCombinations(k, subList))
                        {
                            yield return combination;
                        }
                    }
                }
            }

            return (source) =>
            {
                var list = source.ToList();
                return k >= list.Count()
                    ? Enumerable.Empty<IEnumerable<T>>()
                    : AllCombinations(k, list);
            };
        }

        /// <summary>
        /// Creates a shrink function that shrinks a source enumerable towards a target count, like
        /// <see cref="Towards(int)"/>. Whilst shrinking the enumerable, it will generate combinations for a count from
        /// the original source.
        /// </summary>
        /// <param name="target">The count to shrink towards.</param>
        /// <returns>A shrink function for enumerables.</returns>
        public static ShrinkFunc<IEnumerable<T>> TowardsCount<T>(int target)
        {
            var towardsCount = Towards(target);

            return (source) =>
            {
                var lengths = towardsCount(source.Count());

                var shrinksForSource = lengths.Select(length => source.Take(length));

                var shrinksForCombinations = lengths.SelectMany(length =>
                {
                    // Skip the first, as we already have this combination from shrinksForSource
                    return OtherCombinations<T>(length)(source).Skip(1);
                });

                return Enumerable.Concat(shrinksForSource, shrinksForCombinations);
            };
        }

        private static IEnumerable<int> Halves(int value) =>
            EnumerableExtensions
                .Unfold(Math.Abs(value), x =>
                {
                    var x0 = x / 2;
                    return x0 > 0 ? new Option.Some<int>(x0) : new Option.None<int>();
                })
                .Select(x => Math.Sign(value) == -1 ? -x : x);
    }
}
