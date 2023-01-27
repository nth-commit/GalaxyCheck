using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.Iterations
{
    public interface IGenIteration
    {
        GenParameters ReplayParameters { get; }

        GenParameters NextParameters { get; }

        IGenData Data { get; }
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

    public interface IGenInstanceData
    {
        IExampleSpace ExampleSpace { get; }
    }

    public interface IGenErrorData
    {
        string Message { get; }
    }

    public interface IGenDiscardData
    {
        IExampleSpace ExampleSpace { get; }
    }

    public record GenInstanceData(IExampleSpace ExampleSpace) : IGenInstanceData;

    public record GenErrorData(string Message) : IGenErrorData;

    public record GenDiscardData(IExampleSpace ExampleSpace) : IGenDiscardData;

    public record GenData : IGenData
    {
        public static GenData InstanceData(IExampleSpace exampleSpace) => new GenData
        {
            Instance = new GenInstanceData(exampleSpace),
            Error = null,
            Discard = null
        };

        public static GenData ErrorData(string message) => new GenData
        {
            Instance = null,
            Error = new GenErrorData(message),
            Discard = null
        };

        public static GenData DiscardData(IExampleSpace exampleSpace) => new GenData
        {
            Instance = null,
            Error = null,
            Discard = new GenDiscardData(exampleSpace)
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
