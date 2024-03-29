﻿using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;

namespace GalaxyCheck.Gens.Iterations
{
    public interface IGenIteration
    {
        GenParameters ReplayParameters { get; }

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
        GenParameters NextParameters { get; }

        IExampleSpace ExampleSpace { get; }
    }

    public interface IGenErrorData
    {
        string Message { get; }
    }

    public interface IGenDiscardData
    {
        GenParameters NextParameters { get; }

        IExampleSpace ExampleSpace { get; }
    }

    public record GenInstanceData(GenParameters NextParameters, IExampleSpace ExampleSpace) : IGenInstanceData;

    public record GenErrorData(string Message) : IGenErrorData;

    public record GenDiscardData(GenParameters NextParameters, IExampleSpace ExampleSpace) : IGenDiscardData;

    public record GenData : IGenData
    {
        public static GenData InstanceData(GenParameters nextParameters, IExampleSpace exampleSpace) => new GenData
        {
            Instance = new GenInstanceData(nextParameters, exampleSpace),
            Error = null,
            Discard = null
        };

        public static GenData ErrorData(string message) => new GenData
        {
            Instance = null,
            Error = new GenErrorData(message),
            Discard = null
        };

        public static GenData DiscardData(GenParameters nextParameters, IExampleSpace exampleSpace) => new GenData
        {
            Instance = null,
            Error = null,
            Discard = new GenDiscardData(nextParameters, exampleSpace)
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
