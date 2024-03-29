﻿using GalaxyCheck.ExampleSpaces;
using System;
using GalaxyCheck.Gens.Parameters;

namespace GalaxyCheck.Gens.Iterations.Generic
{
    public interface IGenIteration<out T> : IGenIteration
    {
        new IGenData<T> Data { get; }

        TResult Match<TResult>(
            Func<IGenInstance<T>, TResult> onInstance,
            Func<IGenError<T>, TResult> onError,
            Func<IGenDiscard<T>, TResult> onDiscard);
    }

    public interface IGenData<out T>
    {
        IGenInstanceData<T>? Instance { get; }

        IGenErrorData? Error { get; }

        IGenDiscardData? Discard { get; }

        TResult Match<TResult>(
            Func<IGenInstanceData<T>, TResult> onInstance,
            Func<IGenErrorData, TResult> onError,
            Func<IGenDiscardData, TResult> onDiscard);
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

    public interface IGenInstanceData<out T> : IGenInstanceData
    {
        new IExampleSpace<T> ExampleSpace { get; }
    }

    public record GenInstanceData<T>(GenParameters NextParameters, IExampleSpace<T> ExampleSpace) : IGenInstanceData<T>
    {
        IExampleSpace IGenInstanceData.ExampleSpace => ExampleSpace;
    }

    public record GenErrorData(string Message) : IGenErrorData;

    public record GenDiscardData(GenParameters NextParameters, IExampleSpace ExampleSpace) : IGenDiscardData;

    public record GenData<T> : IGenData<T>
    {
        public static GenData<T> InstanceData(GenParameters nextParameters, IExampleSpace<T> exampleSpace) =>
            new GenData<T>
            {
                Instance = new GenInstanceData<T>(nextParameters, exampleSpace),
                Error = null,
                Discard = null
            };

        public static GenData<T> ErrorData(string message) => new GenData<T>
        {
            Instance = null,
            Error = new GenErrorData(message),
            Discard = null
        };

        public static GenData<T> DiscardData(GenParameters nextParameters, IExampleSpace exampleSpace) => new GenData<T>
        {
            Instance = null,
            Error = null,
            Discard = new GenDiscardData(nextParameters, exampleSpace)
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
