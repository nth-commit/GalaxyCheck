using GalaxyCheck.Internal.ExampleSpaces;
using System;

namespace GalaxyCheck.Internal.GenIterations.Data
{
    public interface IGenInstanceData
    {
        IExampleSpace ExampleSpace { get; }
    }

    public interface IGenErrorData
    {
        string GenName { get; }

        string Message { get; }
    }

    public interface IGenDiscardData
    {
    }

    public interface IGenData
    {
        IGenInstanceData? Instance { get; }

        IGenDiscardData? Discard { get; }

        IGenErrorData? Error { get; }

        T Match<T>(
            Func<IGenInstanceData, T> onInstance,
            Func<IGenErrorData, T> onError,
            Func<IGenDiscardData, T> onDiscard);
    }

    public record GenInstanceData(IExampleSpace ExampleSpace) : IGenInstanceData;

    public record GenErrorData(string GenName, string Message) : IGenErrorData;

    public record GenDiscardData() : IGenDiscardData;

    public record GenData : IGenData
    {
        public static GenData InstanceData(IExampleSpace exampleSpace) => new GenData
        {
            Instance = new GenInstanceData(exampleSpace),
            Error = null,
            Discard = null
        };

        public static GenData ErrorData(string genName, string message) => new GenData
        {
            Instance = null,
            Error = new GenErrorData(genName, message),
            Discard = null
        };

        public static GenData DiscardData() => new GenData
        {
            Instance = null,
            Error = null,
            Discard = new GenDiscardData()
        };

        public IGenInstanceData? Instance { get; init; }

        public IGenErrorData? Error { get; init; }

        public IGenDiscardData? Discard { get; init; }

        public T Match<T>(
            Func<IGenInstanceData, T> onInstance,
            Func<IGenErrorData, T> onError,
            Func<IGenDiscardData, T> onDiscard) =>
                Instance != null ? onInstance(Instance) :
                Error != null ? onError(Error) :
                Discard != null ? onDiscard(Discard) :
                throw new NotSupportedException();
    }
}

namespace GalaxyCheck.Internal.GenIterations.Data.Generic
{
    public interface IGenInstanceData<out T> : IGenInstanceData
    {
        new IExampleSpace<T> ExampleSpace { get; }
    }

    public interface IGenData<out T>
    {
        IGenInstanceData<T>? Instance { get; }

        IGenDiscardData? Discard { get; }

        IGenErrorData? Error { get; }

        TResult Match<TResult>(
            Func<IGenInstanceData<T>, TResult> onInstance,
            Func<IGenErrorData, TResult> onError,
            Func<IGenDiscardData, TResult> onDiscard);
    }

    public record GenInstanceData<T> : IGenInstanceData<T>
    {
        public GenInstanceData(IExampleSpace<T> exampleSpace)
        {
            ExampleSpace = exampleSpace;
        }

        public IExampleSpace<T> ExampleSpace { get; init; }

        IExampleSpace IGenInstanceData.ExampleSpace => ExampleSpace;
    }

    public record GenErrorData(string GenName, string Message) : IGenErrorData;

    public record GenDiscardData() : IGenDiscardData;

    public record GenData<T> : IGenData<T>
    {
        public static GenData<T> InstanceData(IExampleSpace<T> exampleSpace) => new GenData<T>
        {
            Instance = new GenInstanceData<T>(exampleSpace),
            Error = null,
            Discard = null
        };

        public static GenData<T> ErrorData(string genName, string message) => new GenData<T>
        {
            Instance = null,
            Error = new GenErrorData(genName, message),
            Discard = null
        };

        public static GenData<T> DiscardData() => new GenData<T>
        {
            Instance = null,
            Error = null,
            Discard = new GenDiscardData()
        };

        public IGenInstanceData<T>? Instance { get; init; }

        public IGenErrorData? Error { get; init; }

        public IGenDiscardData? Discard { get; init; }

        public TResult Match<TResult>(
            Func<IGenInstanceData<T>, TResult> onInstance,
            Func<IGenErrorData, TResult> onError,
            Func<IGenDiscardData, TResult> onDiscard) =>
                Instance != null ? onInstance(Instance) :
                Error != null ? onError(Error) :
                Discard != null ? onDiscard(Discard) :
                throw new NotSupportedException();
    }
}

namespace GalaxyCheck.Internal.GenIterations
{
    using GalaxyCheck.Internal.GenIterations.Data;
    using GalaxyCheck.Internal.GenIterations.Data.Generic;
    using GalaxyCheck.Internal.Utility;

    public interface IGenIteration
    {
        GenParameters RepeatParameters { get; }

        GenParameters NextParameters { get; }

        IGenData Data { get; }
    }

    public record GenIteration2(
        GenParameters RepeatParameters,
        GenParameters NextParameters,
        IGenData Data) : IGenIteration;

    public interface IGenIteration<out T> : IGenIteration
    {
        new IGenData<T> Data { get; }

        TResult Match<TResult>(
            Func<IGenInstance<T>, TResult> onInstance,
            Func<IGenError<T>, TResult> onError,
            Func<IGenDiscard<T>, TResult> onDiscard);
    }

    public interface IGenInstance<out T> : IGenIteration<T>, IGenInstanceData<T>
    {
    }

    public interface IGenError<out T> : IGenIteration<T>, IGenErrorData
    {
    }

    public interface IGenDiscard<out T> : IGenIteration<T>, IGenDiscardData
    {
    }

    public static class GenIterationFactory
    {
        private record GenIteration<T> : IGenIteration<T>
        {
            public GenIteration(
                GenParameters repeatParameters,
                GenParameters nextParameters,
                IGenData<T> data)
            {
                RepeatParameters = repeatParameters;
                NextParameters = nextParameters;
                Data = data;
            }

            public GenParameters RepeatParameters { get; init; }

            public GenParameters NextParameters { get; init; }

            public IGenData<T> Data { get; init; }

            IGenData IGenIteration.Data => Data.Match<IGenData>(
                onInstance: instanceData => GenData.InstanceData(instanceData.ExampleSpace),
                onError: errorData => GenData.ErrorData(errorData.GenName, errorData.Message),
                onDiscard: discardData => GenData.DiscardData());

            public TResult Match<TResult>(
                Func<IGenInstance<T>, TResult> onInstance,
                Func<IGenError<T>, TResult> onError,
                Func<IGenDiscard<T>, TResult> onDiscard) => Data.Match(
                    onInstance: _ => onInstance(new GenInstance<T>(RepeatParameters, NextParameters, Data)),
                    onError: _ => onError(new GenError<T>(RepeatParameters, NextParameters, Data)),
                    onDiscard: _ => onDiscard(new GenDiscard<T>(RepeatParameters, NextParameters, Data)));

            private record GenInstance<U> : GenIteration<U>, IGenInstance<U>
            {
                public GenInstance(
                    GenParameters repeatParameters,
                    GenParameters nextParameters,
                    IGenData<U> data)
                    : base(repeatParameters, nextParameters, data)
                {
                }

                public IExampleSpace<U> ExampleSpace => Data.Instance!.ExampleSpace;

                IExampleSpace IGenInstanceData.ExampleSpace => ExampleSpace;
            }

            private record GenError<U> : GenIteration<U>, IGenError<U>
            {
                public GenError(
                    GenParameters repeatParameters,
                    GenParameters nextParameters,
                    IGenData<U> data)
                    : base(repeatParameters, nextParameters, data)
                {
                }

                public string GenName => Data.Error!.GenName;

                public string Message => Data.Error!.Message;
            }

            private record GenDiscard<U> : GenIteration<U>, IGenDiscard<U>
            {
                public GenDiscard(
                    GenParameters repeatParameters,
                    GenParameters nextParameters,
                    IGenData<U> data)
                    : base(repeatParameters, nextParameters, data)
                {
                }
            }
        }

        public static IGenIteration<T> Instance<T>(
            GenParameters repeatParameters,
            GenParameters nextParameters,
            IExampleSpace<T> exampleSpace) =>
                new GenIteration<T>(repeatParameters, nextParameters, GenData<T>.InstanceData(exampleSpace));

        public static IGenIteration<T> Error<T>(
            GenParameters repeatParameters,
            GenParameters nextParameters,
            string genName,
            string message) =>
                new GenIteration<T>(repeatParameters, nextParameters, GenData<T>.ErrorData(genName, message));

        public static IGenIteration<T> Discard<T>(
            GenParameters repeatParameters,
            GenParameters nextParameters) =>
                new GenIteration<T>(repeatParameters, nextParameters, GenData<T>.DiscardData());
    }

    public static class GenIterationExtensions
    {
        public static Either<IGenInstance<TSource>, IGenIteration<TNonInstance>> ToEither<TSource, TNonInstance>(
            this IGenIteration<TSource> iteration) => iteration.Match<Either<IGenInstance<TSource>, IGenIteration<TNonInstance>>>(
                onInstance: instance => new Left<IGenInstance<TSource>, IGenIteration<TNonInstance>>(instance),
                onError: error => new Right<IGenInstance<TSource>, IGenIteration<TNonInstance>>(
                    GenIterationFactory.Error<TNonInstance>(error.RepeatParameters, error.NextParameters, error.GenName, error.Message)),
                onDiscard: discard => new Right<IGenInstance<TSource>, IGenIteration<TNonInstance>>(
                    GenIterationFactory.Discard<TNonInstance>(discard.RepeatParameters, discard.NextParameters)));

        public static bool IsInstance<T>(this IGenIteration<T> iteration) => iteration.Match(
            onInstance: _ => true,
            onError: _ => false,
            onDiscard: _ => false);

        public static bool IsDiscard<T>(this IGenIteration<T> iteration) => iteration.Match(
            onInstance: _ => false,
            onError: _ => false,
            onDiscard: _ => true);
    }
}
