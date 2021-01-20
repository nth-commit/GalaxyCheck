using GalaxyCheck.Internal.GenIterations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Internal.Utility
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> UnfoldInfinite<T>(T seed, Func<T, T> generateNext) =>
            Unfold(seed, x => new Option.Some<T>(generateNext(x)));

        public static IEnumerable<T> Unfold<T>(T seed, Func<T, Option<T>> tryGenerateNext)
        {
            var current = seed;
            while (true)
            {
                yield return current;

                if (tryGenerateNext(current) is Option.Some<T> some)
                {
                    current = some.Value;
                }
                else
                {
                    break;
                }

            }
        }

        public static IEnumerable<T> UnfoldManyInfinite<T>(IEnumerable<T> seed, Func<T, IEnumerable<T>> generateNext) =>
            UnfoldMany(seed, x => new Option.Some<IEnumerable<T>>(generateNext(x)));

        public static IEnumerable<T> UnfoldMany<T>(IEnumerable<T> seed, Func<T, Option<IEnumerable<T>>> tryGenerateNext)
        {
            var current = seed;
            while (true)
            {
                if (!current.Any())
                {
                    throw new Exception("Fatal: UnfoldMany expected a non-empty enumerable");
                }

                foreach (var element in current)
                {
                    yield return element;
                }

                if (tryGenerateNext(current.Last()) is Option.Some<IEnumerable<T>> some)
                {
                    current = some.Value;
                }
                else
                {
                    break;
                }
            }
        }

        public static IEnumerable<T> Repeat<T>(Func<IEnumerable<T>> generate)
        {
            while (true)
            {
                foreach (var element in generate())
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TAccumulator> Scan<TSource, TAccumulator>(
            this IEnumerable<TSource> input,
            TAccumulator seed,
            Func<TAccumulator, TSource, TAccumulator> next)
        {
            yield return seed;
            foreach (var item in input)
            {
                seed = next(seed, item);
                yield return seed;
            }
        }

        public static IEnumerable<(TSource element, TAccumulator state)> ScanInParallel<TSource, TAccumulator>(
            this IEnumerable<TSource> input,
            TAccumulator seed,
            Func<TAccumulator, TSource, TAccumulator> next)
        {
            var state = seed;
            foreach (var item in input)
            {
                state = next(state, item);
                yield return (item, state);
            }
        }

        public static IEnumerable<T> TakeWhileInclusive<T>(
            this IEnumerable<T> source,
            Func<T, bool> pred)
        {
            foreach (var element in source)
            {
                yield return element;

                if (!pred(element))
                {
                    break;
                }
            }
        }

        public static IEnumerable<T> Tap<T>(
            this IEnumerable<T> source,
            Action<T> action)
        {
            foreach (var element in source)
            {
                action(element);
                yield return element;
            }
        }

        public static IEnumerable<(GenIteration<T> iteration, int consecutiveDiscards)> WithConsecutiveDiscardCount<T>(
            this IEnumerable<GenIteration<T>> source)
        {
            return source
                .Scan(
                    (iteration: (GenIteration<T>?)null!, consecutiveDiscards: 0),
                    (acc, curr) =>
                    {
                        var consecutiveDiscards = curr is GenDiscard<T> ? acc.consecutiveDiscards + 1 : 0;
                        return new (curr, consecutiveDiscards);
                    })
                .Skip(1);
        }
    }
}
