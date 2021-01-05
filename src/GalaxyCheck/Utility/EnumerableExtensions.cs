using System;
using System.Collections.Generic;

namespace GalaxyCheck.Utility
{
    internal static class EnumerableExtensions
    {
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
    }
}
