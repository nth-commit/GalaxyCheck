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
        public static IFunctionGen<TResult> Function<TResult>(IGen<TResult> returnGen) =>
            new FunctionGen<TResult>(returnGen);

        public static IFunctionGen<T0, TResult> Function<T0, TResult>(IGen<TResult> returnGen) =>
            new FunctionGen<T0, TResult>(returnGen);

        public static IFunctionGen<T0, T1, TResult> Function<T0, T1, TResult>(IGen<TResult> returnGen) =>
            new FunctionGen<T0, T1, TResult>(returnGen);

        public static IFunctionGen<T0, T1, T2, TResult> Function<T0, T1, T2, TResult>(IGen<TResult> returnGen) =>
            new FunctionGen<T0, T1, T2, TResult>(returnGen);

        public static IFunctionGen<T0, T1, T2, T3, TResult> Function<T0, T1, T2, T3, TResult>(IGen<TResult> returnGen) =>
            new FunctionGen<T0, T1, T2, T3, TResult>(returnGen);
    }
}

namespace GalaxyCheck.Gens
{
    public interface IFunctionGen<TResult> : IGen<Func<TResult>>
    {
        IFunctionGen<TResult> WithInvocationLimit(int limit);

        IFunctionGen<TResult> WithoutInvocationLimit();
    }

    public class FunctionGen<TResult> : GenProvider<Func<TResult>>, IFunctionGen<TResult>
    {
        private readonly IGen<TResult> _returnGen;
        private readonly int? _invocationLimit;

        private FunctionGen(IGen<TResult> returnGen, int? invocationLimit)
        {
            _returnGen = returnGen;
            _invocationLimit = invocationLimit;
        }

        public FunctionGen(IGen<TResult> returnGen)
            : this(returnGen, 1000)
        {
        }

        public IFunctionGen<TResult> WithInvocationLimit(int limit) => new FunctionGen<TResult>(_returnGen, limit);

        public IFunctionGen<TResult> WithoutInvocationLimit() => new FunctionGen<TResult>(_returnGen, null);

        protected override IGen<Func<TResult>> Gen =>
            from f in new VariadicFunctionGen<TResult>(_returnGen, _invocationLimit)
            select new Func<TResult>(() => f());
    }

    public interface IFunctionGen<T0, TResult> : IGen<Func<T0, TResult>>
    {
        IFunctionGen<T0, TResult> WithInvocationLimit(int limit);

        IFunctionGen<T0, TResult> WithoutInvocationLimit();
    }

    public class FunctionGen<T0, TResult> : GenProvider<Func<T0, TResult>>, IFunctionGen<T0, TResult>
    {
        private readonly IGen<TResult> _returnGen;
        private readonly int? _invocationLimit;

        private FunctionGen(IGen<TResult> returnGen, int? invocationLimit)
        {
            _returnGen = returnGen;
            _invocationLimit = invocationLimit;
        }

        public FunctionGen(IGen<TResult> returnGen)
            : this(returnGen, 1000)
        {
        }

        public IFunctionGen<T0, TResult> WithInvocationLimit(int limit) => new FunctionGen<T0, TResult>(_returnGen, limit);

        public IFunctionGen<T0, TResult> WithoutInvocationLimit() => new FunctionGen<T0, TResult>(_returnGen, null);

        protected override IGen<Func<T0, TResult>> Gen =>
            from f in new VariadicFunctionGen<TResult>(_returnGen, _invocationLimit)
            select new Func<T0, TResult>((arg0) => f(arg0));
    }

    public interface IFunctionGen<T0, T1, TResult> : IGen<Func<T0, T1, TResult>>
    {
        IFunctionGen<T0, T1, TResult> WithInvocationLimit(int limit);

        IFunctionGen<T0, T1, TResult> WithoutInvocationLimit();
    }

    public class FunctionGen<T0, T1, TResult> : GenProvider<Func<T0, T1, TResult>>, IFunctionGen<T0, T1, TResult>
    {
        private readonly IGen<TResult> _returnGen;
        private readonly int? _invocationLimit;

        private FunctionGen(IGen<TResult> returnGen, int? invocationLimit)
        {
            _returnGen = returnGen;
            _invocationLimit = invocationLimit;
        }

        public FunctionGen(IGen<TResult> returnGen)
            : this(returnGen, 1000)
        {
        }

        public IFunctionGen<T0, T1, TResult> WithInvocationLimit(int limit) => new FunctionGen<T0, T1, TResult>(_returnGen, limit);

        public IFunctionGen<T0, T1, TResult> WithoutInvocationLimit() => new FunctionGen<T0, T1, TResult>(_returnGen, null);

        protected override IGen<Func<T0, T1, TResult>> Gen =>
            from f in new VariadicFunctionGen<TResult>(_returnGen, _invocationLimit)
            select new Func<T0, T1, TResult>((arg0, arg1) => f(arg0, arg1));
    }

    public interface IFunctionGen<T0, T1, T2, TResult> : IGen<Func<T0, T1, T2, TResult>>
    {
        IFunctionGen<T0, T1, T2, TResult> WithInvocationLimit(int limit);

        IFunctionGen<T0, T1, T2, TResult> WithoutInvocationLimit();
    }

    public class FunctionGen<T0, T1, T2, TResult> : GenProvider<Func<T0, T1, T2, TResult>>, IFunctionGen<T0, T1, T2, TResult>
    {
        private readonly IGen<TResult> _returnGen;
        private readonly int? _invocationLimit;

        private FunctionGen(IGen<TResult> returnGen, int? invocationLimit)
        {
            _returnGen = returnGen;
            _invocationLimit = invocationLimit;
        }

        public FunctionGen(IGen<TResult> returnGen)
            : this(returnGen, 1000)
        {
        }

        public IFunctionGen<T0, T1, T2, TResult> WithInvocationLimit(int limit) => new FunctionGen<T0, T1, T2, TResult>(_returnGen, limit);

        public IFunctionGen<T0, T1, T2, TResult> WithoutInvocationLimit() => new FunctionGen<T0, T1, T2, TResult>(_returnGen, null);

        protected override IGen<Func<T0, T1, T2, TResult>> Gen =>
            from f in new VariadicFunctionGen<TResult>(_returnGen, _invocationLimit)
            select new Func<T0, T1, T2, TResult>((arg0, arg1, arg2) => f(arg0, arg1, arg2));
    }

    public interface IFunctionGen<T0, T1, T2, T3, TResult> : IGen<Func<T0, T1, T2, T3, TResult>>
    {
        IFunctionGen<T0, T1, T2, T3, TResult> WithInvocationLimit(int limit);

        IFunctionGen<T0, T1, T2, T3, TResult> WithoutInvocationLimit();
    }

    public class FunctionGen<T0, T1, T2, T3, TResult> : GenProvider<Func<T0, T1, T2, T3, TResult>>, IFunctionGen<T0, T1, T2, T3, TResult>
    {
        private readonly IGen<TResult> _returnGen;
        private readonly int? _invocationLimit;

        private FunctionGen(IGen<TResult> returnGen, int? invocationLimit)
        {
            _returnGen = returnGen;
            _invocationLimit = invocationLimit;
        }

        public FunctionGen(IGen<TResult> returnGen)
            : this(returnGen, 1000)
        {
        }

        public IFunctionGen<T0, T1, T2, T3, TResult> WithInvocationLimit(int limit) => new FunctionGen<T0, T1, T2, T3, TResult>(_returnGen, limit);

        public IFunctionGen<T0, T1, T2, T3, TResult> WithoutInvocationLimit() => new FunctionGen<T0, T1, T2, T3, TResult>(_returnGen, null);

        protected override IGen<Func<T0, T1, T2, T3, TResult>> Gen =>
            from f in new VariadicFunctionGen<TResult>(_returnGen, _invocationLimit)
            select new Func<T0, T1, T2, T3, TResult>((arg0, arg1, arg2, arg3) => f(arg0, arg1, arg2, arg3));
    }

    internal delegate TResult VariadicFunc<TResult>(params object?[] args);

    internal class VariadicFunctionGen<TResult> : LazyGen<VariadicFunc<TResult>>
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
            var interfaceIdentifier = "IFunctionGen";
            var withLimitMethodIdentifier = $"{interfaceIdentifier}.{nameof(IFunctionGen<object>.WithInvocationLimit)}";
            var withoutLimitMethodIdentifier = $"{interfaceIdentifier}.{nameof(IFunctionGen<object>.WithoutInvocationLimit)}";
            var message = $"Function exceeded invocation limit. This is a built-in safety mechanism to prevent hanging tests. Use {withLimitMethodIdentifier} or {withoutLimitMethodIdentifier} to modify this limit.";
            throw new Exceptions.GenLimitExceededException(message);
        }

        private static int HashArguments(params object?[] args) => args.Aggregate(0, (acc, curr) => HashCode.Combine(acc, curr));
    }
}