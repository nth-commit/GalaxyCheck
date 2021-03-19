using GalaxyCheck.Internal.ExampleSpaces;
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

        IEnumerable<IExampleSpace> ExampleSpaceHistory { get; }
    }

    public interface IGenErrorData
    {
        string GenName { get; }

        string Message { get; }
    }

    public interface IGenDiscardData
    {
    }

    public record GenInstanceData(
        IExampleSpace ExampleSpace,
        IEnumerable<IExampleSpace> ExampleSpaceHistory) : IGenInstanceData;

    public record GenErrorData(string GenName, string Message) : IGenErrorData;

    public record GenDiscardData() : IGenDiscardData;

    public record GenData : IGenData
    {
        public static GenData InstanceData(
            IExampleSpace exampleSpace,
            IEnumerable<IExampleSpace> exampleSpaceHistory) => new GenData
            {
                Instance = new GenInstanceData(exampleSpace, exampleSpaceHistory),
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
