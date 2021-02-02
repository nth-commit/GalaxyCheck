using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IGen<Func<TResult>> Function<TResult>(IGen<TResult> returnGen) =>
            from f in VariadicFunction(returnGen)
            select new Func<TResult>(() => f());

        public static IGen<Func<T0, TResult>> Function<T0, TResult>(IGen<TResult> returnGen) => 
            from f in VariadicFunction(returnGen)
            select new Func<T0, TResult>((arg0) => f(arg0));

        public static IGen<Func<T0, T1, TResult>> Function<T0, T1, TResult>(IGen<TResult> returnGen) =>
            from f in VariadicFunction(returnGen)
            select new Func<T0, T1, TResult>((arg0, arg1) => f(arg0, arg1));

        public static IGen<Func<T0, T1, T2, TResult>> Function<T0, T1, T2, TResult>(IGen<TResult> returnGen) =>
            from f in VariadicFunction(returnGen)
            select new Func<T0, T1, T2, TResult>((arg0, arg1, arg2) => f(arg0, arg1, arg2));

        public static IGen<Func<T0, T1, T2, T3, TResult>> Function<T0, T1, T2, T3, TResult>(IGen<TResult> returnGen) =>
            from f in VariadicFunction(returnGen)
            select new Func<T0, T1, T2, T3, TResult>((arg0, arg1, arg2, arg3) => f(arg0, arg1, arg2, arg3));

        private delegate TResult VariadicFunc<TResult>(params object?[] args);

        private static IGen<VariadicFunc<TResult>> VariadicFunction<TResult>(IGen<TResult> returnGen) =>
            from returnValueSource in returnGen.InfiniteOf()
            select ToPureFunction(returnValueSource);

        /// <summary>
        /// When the function is called, it will pull a newly generated value from the source. Hacks purity into the
        /// function by memoizing the result. Without memoization, repeating a function call with the same args would
        /// continue to pull values from the source, likely generated a different result.
        /// </summary>
        private static VariadicFunc<TResult> ToPureFunction<TResult>(IEnumerable<TResult> returnValueSource)
        {
            var returnValueCache = new ConcurrentDictionary<int, TResult>();
            var returnValueEnumerator = returnValueSource.GetEnumerator();

            return new VariadicFunc<TResult>(args =>
            {
                var key = HashArguments(args);

                return returnValueCache.GetOrAdd(key, (_) =>
                {
                    returnValueEnumerator.MoveNext();
                    return returnValueEnumerator.Current;
                });
            });
        }

        private static int HashArguments(params object?[] args) => args.Aggregate(0, (acc, curr) => HashCode.Combine(acc, curr));
    }
}