using GalaxyCheck.Gens;
using GalaxyCheck.Internal.Gens;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IGen<Func<TResult>> Function<TResult>(
            IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                VariadicFunction(returnGen, invocationLimit)
                    .Select<VariadicFunc<TResult>, Func<TResult>>(
                        f => () => f());

        public static IGen<Func<T0, TResult>> Function<T0, TResult>(
            IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                VariadicFunction(returnGen, invocationLimit)
                    .Select<VariadicFunc<TResult>, Func<T0, TResult>>(
                        f => (arg0) => f(arg0));

        public static IGen<Func<T0, T1, TResult>> Function<T0, T1, TResult>(
            IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                VariadicFunction(returnGen, invocationLimit)
                    .Select<VariadicFunc<TResult>, Func<T0, T1, TResult>>(
                        f => (arg0, arg1) => f(arg0, arg1));

        public static IGen<Func<T0, T1, T2, TResult>> Function<T0, T1, T2, TResult>(
            IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                VariadicFunction(returnGen, invocationLimit)
                    .Select<VariadicFunc<TResult>, Func<T0, T1, T2, TResult>>(
                        f => (arg0, arg1, arg2) => f(arg0, arg1, arg2));

        public static IGen<Func<T0, T1, T2, T3, TResult>> Function<T0, T1, T2, T3, TResult>(
            IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                VariadicFunction(returnGen, invocationLimit)
                    .Select<VariadicFunc<TResult>, Func<T0, T1, T2, T3, TResult>>(
                        f => (arg0, arg1, arg2, arg3) => f(arg0, arg1, arg2, arg3));

        private static IGen<VariadicFunc<TResult>> VariadicFunction<TResult>(IGen<TResult> returnGen, int? invocationLimit) =>
            new VariadicFunctionGen<TResult>(returnGen, invocationLimit);
    }

    public static partial class Extensions
    {
        public static IGen<Func<TResult>> FunctionOf<TResult>(
            this IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                Gen.Function(returnGen, invocationLimit);

        public static IGen<Func<T0, TResult>> FunctionOf<T0, TResult>(
            this IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                Gen.Function<T0, TResult>(returnGen, invocationLimit);

        public static IGen<Func<T0, T1, TResult>> FunctionOf<T0, T1, TResult>(
            this IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                Gen.Function<T0, T1, TResult>(returnGen, invocationLimit);

        public static IGen<Func<T0, T1, T2, TResult>> FunctionOf<T0, T1, T2, TResult>(
            this IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                Gen.Function<T0, T1, T2, TResult>(returnGen, invocationLimit);

        public static IGen<Func<T0, T1, T2, T3, TResult>> FunctionOf<T0, T1, T2, T3, TResult>(
            this IGen<TResult> returnGen,
            int? invocationLimit = 1000) =>
                Gen.Function<T0, T1, T2, T3, TResult>(returnGen, invocationLimit);
    }
}

namespace GalaxyCheck.Gens
{
    public delegate TResult VariadicFunc<out TResult>(params object?[] args);

    public class VariadicFunctionGen<TResult> : LazyGen<VariadicFunc<TResult>>
    {
        public VariadicFunctionGen(IGen<TResult> returnGen, int? invocationLimit)
            : base(() => Create(returnGen, invocationLimit))
        {
        }

        public static IGen<VariadicFunc<TResult>> Create(IGen<TResult> returnGen, int? invocationLimit) =>
            from returnValueSource in returnGen.InfiniteOf(iterationLimit: null)
            select ToPureFunction(returnValueSource, invocationLimit);

        /// <summary>
        /// When the function is called, it will pull a newly generated value from the source. Hacks purity into the
        /// function by memoizing the result. Without memoization, repeating a function call with the same args would
        /// continue to pull values from the source, likely generating a different result.
        /// </summary>
        private static VariadicFunc<TResult> ToPureFunction(IEnumerable<TResult> returnValueSource, int? optionalInvocationLimit)
        {
            var returnValueCache = new ConcurrentDictionary<int, TResult>();
            var returnValueEnumerator = returnValueSource.GetEnumerator();

            var function = new VariadicFunc<TResult>(args =>
            {
                var key = HashArguments(args);

                return returnValueCache.GetOrAdd(key, (_) =>
                {
                    returnValueEnumerator.MoveNext();
                    return returnValueEnumerator.Current;
                });
            });

            if (optionalInvocationLimit == null)
            {
                return function;
            }

            int invocationCount = 0;
            int invocationLimit = optionalInvocationLimit.Value;

            return new VariadicFunc<TResult>(args =>
            {
                Interlocked.Increment(ref invocationCount);

                // TODO: Threadsafe
                if (invocationCount > invocationLimit)
                {
                    ThrowGenLimitExceeded();
                }

                return function(args);
            });
        }

        private static void ThrowGenLimitExceeded()
        {
            var message = "Function exceeded invocation limit. This is a built-in safety mechanism to prevent hanging tests. Relax this constraint by configuring the invocationLimit parameter.";
            throw new Exceptions.GenLimitExceededException(message);
        }

        private static int HashArguments(params object?[] args) => args.Aggregate(0, (acc, curr) => HashCode.Combine(acc, curr));
    }
}