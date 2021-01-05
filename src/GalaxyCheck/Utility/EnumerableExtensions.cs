using System;
using System.Collections.Generic;

namespace GalaxyCheck.Utility
{
    public static class EnumerableExtensions
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
    }
}
