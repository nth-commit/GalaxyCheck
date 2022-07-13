using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

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
    }
}
