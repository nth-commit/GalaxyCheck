using GalaxyCheck;
using GalaxyCheck.Internal;
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

    public delegate (TContext, IEnumerable<T>) ContextualShrinkFunc<T, TContext>(T value, TContext ctx);

    public delegate TContext NextContextFunc<T, TContext>(T value, TContext ctx);

    public record NoContext;

    public interface ContextualShrinker<T, TContext>
    {
        TContext RootContext { get; }

        ContextualShrinkFunc<T, TContext> ContextualShrink { get; }

        NextContextFunc<T, TContext> ContextualTraverse { get; }
    }

    public static class ShrinkFunc
    {
        public static ShrinkFunc<TResult> Map<T, TResult>(
            this ShrinkFunc<T> shrink,
            Func<T, TResult> forwardMapper,
            Func<TResult, T> reverseMapper) =>
                value => shrink(reverseMapper(value)).Select(forwardMapper);

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
        public static ShrinkFunc<int> Towards(int target) => (value) =>
        {
            if (value == target) return Enumerable.Empty<int>();

            // To represent the distance between two signed ints, we need to use something bigger than an int32,
            // because the difference between int.MinValue and int.MaxValue is (approx) int.MaxValue * 2 (which is not
            // representable). We'll need to come up with a better strategy when generating longs.
            var difference = (long)value - target;

            return Halves(difference).Select(smallerDifference => value - smallerDifference).Select(x => (int)x);
        };

        public static ShrinkFunc<long> Towards(long target) => (value) =>
        {
            if (value == target) return Enumerable.Empty<long>();

            // To represent the distance between two signed longs, we need to use something bigger than an long32,
            // because the difference between long.MinValue and long.MaxValue is (approx) long.MaxValue * 2 (which is not
            // representable). We'll need to come up with a better strategy when generating longs.
            var difference = CalculateWidthSafe(value, target);
            var sign = value > target ? -1 : 1;

            return Halves(difference).Select(smallerDifference =>
            {
                if (sign == -1)
                {
                    return value - (long)smallerDifference;
                }
                else
                {
                    return value + (long)smallerDifference;
                }
            });
        };

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

        /// <summary>
        /// Creates a shrink function that shrinks a source list towards a target count, like
        /// <see cref="Towards(int)"/>. Whilst shrinking the enumerable, it will generate combinations for a count from
        /// the original source.
        /// </summary>
        /// <param name="target">The count to shrink towards.</param>
        /// <param name="orderKeySelector">A function to order the list by, in an attempt to generate the first
        /// shrink</param>
        /// <returns>A shrink function for lists.</returns>
        public static ShrinkFunc<List<T>> TowardsCount<T, TKey>(int target, Func<T, TKey> orderKeySelector)
        {
            var order = Order(orderKeySelector);
            var towardsCount = Towards(target);

            return (source) =>
            {
                var shrinksForOrdering = order(source);

                var lengths = towardsCount(source.Count());

                var shrinksForSource = lengths.Select(length => source.Take(length).ToList());

                var shrinksForCombinations = lengths.SelectMany(length =>
                {
                    // Skip the first, as we already have this combination from shrinksForSource
                    return OtherCombinations<T>(length)(source).Skip(1);
                });

                return EnumerableExtensions.Concat(
                    shrinksForOrdering,
                    shrinksForSource,
                    shrinksForCombinations);
            };
        }

        /// <summary>
        /// A second implementation of <see cref="TowardsCount{T, TKey}(int, Func{T, TKey})"/>. Optimized for lists of
        /// greater length than the original, as it will generate shrinks in O(n) fashion, rather than O(n!). The
        /// original implementation would edge the length close to the original value, whilst generating all possible
        /// combinations at that length. This has been replaced by a bisecting phase, which cuts the list in half.
        /// It may not find the optimal shrink in all cases, but it does for all the current test cases. I think we 
        /// can probably improve this later on by using a combination of the two.
        /// </summary>
        /// <param name="target">The count to shrink towards.</param>
        /// <param name="orderKeySelector">A function to order the list by, in an attempt to generate the first
        /// shrink</param>
        /// <returns></returns>
        public static ShrinkFunc<List<T>> TowardsCountOptimized<T, TKey>(int target, Func<T, TKey> orderKeySelector)
        {
            var order = Order(orderKeySelector);
            var towardsCount = Towards(target);
            var bisect = Bisect<T>(target);
            var dropOne = DropOne<T>(target);

            return (source) =>
            {
                var shrinksForOrdering = order(source);

                var withSomeTailElementsDropped =
                    towardsCount(source.Count()).Select(length => source.Take(length).ToList());

                var bisections = bisect(source);

                var withOneElementDropped = dropOne(source);

                return EnumerableExtensions.Concat(
                    shrinksForOrdering,
                    withSomeTailElementsDropped,
                    bisections,
                    withOneElementDropped);
            };
        }

        /// <summary>
        /// Creates a shrink function that generates all the combinations (of n choose k) of the given list. However,
        /// if k >= n, it returns an empty enumerable.
        /// </summary>
        /// <param name="k">The number of elements to choose for a combination.</param>
        /// <returns>A shrink function for enumerables.</returns>
        public static ShrinkFunc<List<T>> OtherCombinations<T>(int k)
        {
            static IEnumerable<List<T>> TailCombinations(int k, List<T> list)
            {
                if (list.Any() == false)
                {
                    return Enumerable.Empty<List<T>>();
                }

                var head = list[0];
                var tail = list.Skip(1).ToList();
                return AllCombinations(k - 1, tail).Select(tail => Enumerable.Concat(new[] { head }, tail).ToList());
            }

            static IEnumerable<List<T>> AllCombinations(int k, List<T> list)
            {
                if (k == 1)
                {
                    foreach (var element in list)
                    {
                        yield return new List<T> { element };
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
                    ? Enumerable.Empty<List<T>>()
                    : AllCombinations(k, list);
            };
        }

        /// <summary>
        /// Creates a shrink function that generates a single sorted shrink if the input is unsorted, but returns no
        /// such shrink if the input is (already) sorted.
        /// </summary>
        /// <param name="keySelector">A function used to select the comparator for the sort.</param>
        /// <returns>A shrink function for enumerables.</returns>
        public static ShrinkFunc<List<T>> Order<T, TKey>(Func<T, TKey> keySelector) => source =>
        {
            var orderedWithInitialIndex = source
                .Select((element, index) => new { element, index })
                .OrderBy(x => keySelector(x.element))
                .ToList();

            var initialIndices = Enumerable.Range(0, orderedWithInitialIndex.Count);
            var orderedIndices = orderedWithInitialIndex.Select(x => x.index);

            var wasOrderByEffectful = orderedIndices.SequenceEqual(initialIndices) == false;

            return wasOrderByEffectful
                ? new[]
                {
                    orderedWithInitialIndex.Select(x => x.element).ToList()
                }
                : Enumerable.Empty<List<T>>();
        };

        /// <summary>
        /// Creates a shrink function that bisect the input list. It does not shrink if the list cannot be bisected
        /// without the resultant lists being less than the minimum length.
        /// </summary>
        /// <param name="minLength">The minimum length for a shrunk list.</param>
        /// <returns>A shrink function for lists.</returns>
        public static ShrinkFunc<List<T>> Bisect<T>(int minLength) => source =>
        {
            var length = source.Count();
            if (length <= 1)
            {
                return Enumerable.Empty<List<T>>();
            }

            var halfLength = length / 2;
            if (halfLength < minLength)
            {
                return Enumerable.Empty<List<T>>();
            }

            return new[]
            {
                source.Take(halfLength).ToList(),
                source.Skip(halfLength).ToList()
            };
        };
        public static ShrinkFunc<List<T>> DropOne<T>(int minLength) => source =>
        {
            if (source.Count <= minLength)
            {
                return Enumerable.Empty<List<T>>();
            }

            if (source.Count == 1)
            {
                return new[] { new List<T>() };
            }

            return Enumerable.Range(0, source.Count).Select(i =>
                Enumerable
                    .Concat(
                        source.Take(i),
                        source.Skip(i + 1))
                    .ToList());
        };

        private static IEnumerable<long> Halves(long value) =>
            EnumerableExtensions
                .Unfold(Math.Abs(value), x =>
                {
                    var x0 = x / 2;
                    return x0 > 0 ? new Option.Some<long>(x0) : new Option.None<long>();
                })
                .Select(x => Math.Sign(value) == -1 ? -x : x);

        private static IEnumerable<ulong> Halves(ulong value) =>
            EnumerableExtensions
                .Unfold(value, x =>
                {
                    var x0 = x / 2;
                    return x0 > 0 ? new Option.Some<ulong>(x0) : new Option.None<ulong>();
                });


        private class NoopContextualShrinker<T> : ContextualShrinker<T, NoContext>
        {
            private readonly ShrinkFunc<T> _shrinker;

            public NoopContextualShrinker(ShrinkFunc<T> shrinker)
            {
                _shrinker = shrinker;
            }

            public NoContext RootContext => new NoContext();

            public ContextualShrinkFunc<T, NoContext> ContextualShrink => (value, context) => (context, _shrinker(value));

            public NextContextFunc<T, NoContext> ContextualTraverse => (_, context) => context;
        }

        public static ContextualShrinker<T, NoContext> WithoutContext<T>(ShrinkFunc<T> shrinker) =>
            new NoopContextualShrinker<T>(shrinker);

        public static ContextualShrinker<T, NoContext> ContextualNone<T>() => WithoutContext<T>(None<T>());
    }
}
