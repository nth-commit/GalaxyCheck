using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Sizing;
using System;

namespace GalaxyCheck.Internal.GenIterations
{
    public interface IGenIteration<out T>
    {
        GenParameters RepeatParameters { get; }

        GenParameters NextParameters { get; }
    }

    public abstract record GenIteration<T> : IGenIteration<T>
    {
        public GenParameters RepeatParameters { get; init; }

        public GenParameters NextParameters { get; init; }

        internal GenIteration(GenParameters RepeatParameters, GenParameters NextParameters)
        {
            this.RepeatParameters = RepeatParameters;
            this.NextParameters = NextParameters;
        }
    }

    public sealed record GenInstance<T>(
        GenParameters RepeatParameters,
        GenParameters NextParameters,
        IExampleSpace<T> ExampleSpace)
            : GenIteration<T>(RepeatParameters, NextParameters);

    public sealed record GenError<T>(
        GenParameters RepeatParameters,
        GenParameters NextParameters,
        string GenName,
        string Message)
            : GenIteration<T>(RepeatParameters, NextParameters);

    public sealed record GenDiscard<T>(
        GenParameters RepeatParameters,
        GenParameters NextParameters)
            : GenIteration<T>(RepeatParameters, NextParameters);

    public static class GenIterationExtensions
    {
        public static TResult Match<T, TResult>(
            this IGenIteration<T> iteration,
            Func<GenInstance<T>, TResult> onInstance,
            Func<GenDiscard<T>, TResult> onDiscard,
            Func<GenError<T>, TResult> onError)
        {
            return iteration switch
            {
                GenInstance<T> instance => onInstance(instance),
                GenDiscard<T> discard => onDiscard(discard),
                GenError<T> error => onError(error),
                _ => throw new NotSupportedException()
            };
        }
    }
}
