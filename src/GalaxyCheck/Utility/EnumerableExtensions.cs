using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Utility
{
    internal static class EnumerableExtensions
    {
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

        public static IEnumerable<T> TakeWhileInclusive<T>(
            this IEnumerable<T> source,
            Func<T, bool> pred)
        {
            var hasPredFailed = false;
            foreach (var element in source)
            {
                yield return element;

                if (hasPredFailed)
                {
                    break;
                }

                hasPredFailed = !pred(element);
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
