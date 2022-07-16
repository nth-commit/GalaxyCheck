using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck.Internal
{
    internal static class AsyncEnumerableExtensions
    {
        public static async IAsyncEnumerable<T> TakeWhileInclusive<T>(
            this IAsyncEnumerable<T> source,
            Func<T, bool> pred)
        {
            await foreach (var element in source)
            {
                yield return element;

                if (!pred(element))
                {
                    break;
                }
            }
        }

        [DebuggerNonUserCode]
        public static async IAsyncEnumerable<T> Unfold<T>(T seed, Func<T, Task<Option<T>>> tryGenerateNext)
        {
            var current = seed;
            while (true)
            {
                yield return current;

                if (await tryGenerateNext(current) is Option.Some<T> some)
                {
                    current = some.Value;
                }
                else
                {
                    break;
                }
            }
        }

        public static async IAsyncEnumerable<(TSource element, TAccumulator state)> ScanInParallel<TSource, TAccumulator>(
            this IAsyncEnumerable<TSource> input,
            TAccumulator seed,
            Func<TAccumulator, TSource, TAccumulator> next)
        {
            var state = seed;
            await foreach (var item in input)
            {
                state = next(state, item);
                yield return (item, state);
            }
        }

        public static IAsyncEnumerable<(T element, int consecutiveDiscardCount)> WithConsecutiveDiscardCount<T>(
            this IAsyncEnumerable<T> source,
            Func<T, bool> isCounted,
            Func<T, bool> isDiscard)
        {
            return source.ScanInParallel(0, (consecutiveDiscardCount, element) =>
            {
                if (isCounted(element) == false)
                {
                    // The element is considered neither a discard or a non-discard, don't increment or reset the
                    // consecutive discard count.
                    return consecutiveDiscardCount;
                }

                if (isDiscard(element))
                {
                    return consecutiveDiscardCount + 1;
                }

                return 0;
            });
        }

        public static IAsyncEnumerable<T> WithDiscardCircuitBreaker<T>(
            this IAsyncEnumerable<T> source,
            Func<T, bool> isCounted,
            Func<T, bool> isDiscard)
        {
            return source
                .WithConsecutiveDiscardCount(isCounted, isDiscard)
                .Select((x) =>
                {
                    var (element, consecutiveDiscardCount) = x;

                    if (consecutiveDiscardCount > 500)
                    {
                        throw new Exceptions.GenExhaustionException();
                    }

                    return element;
                });
        }

        public static async Task<(T? head, IAsyncEnumerable<T> tail)> Deconstruct<T>(this IAsyncEnumerable<T> source)
        {
            var head = await source.FirstOrDefaultAsync();
            var tail = source.Skip(1);
            return (head, tail);
        }
    }
}
