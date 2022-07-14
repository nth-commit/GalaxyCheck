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

        public static async Task<(T? head, IAsyncEnumerable<T> tail)> Deconstruct<T>(this IAsyncEnumerable<T> source)
        {
            var head = await source.FirstOrDefaultAsync();
            var tail = source.Skip(1);
            return (head, tail);
        }
    }
}
