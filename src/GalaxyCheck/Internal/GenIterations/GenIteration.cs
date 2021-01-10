using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Sizing;
using System;

namespace GalaxyCheck.Internal.GenIterations
{
    public abstract record GenIteration<T>
    {
        public IRng InitialRng { get; init; }

        public Size InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public Size NextSize { get; init; }

        internal GenIteration(IRng initialRng, Size initialSize, IRng nextRng, Size nextSize)
        {
            InitialRng = initialRng;
            InitialSize = initialSize;
            NextRng = nextRng;
            NextSize = nextSize;
        }
    }

    public sealed record GenInstance<T>(
        IRng InitialRng,
        Size InitialSize,
        IRng NextRng,
        Size NextSize,
        ExampleSpace<T> ExampleSpace)
            : GenIteration<T>(InitialRng, InitialSize, NextRng, NextSize);

    public sealed record GenError<T>(
        IRng InitialRng,
        Size InitialSize,
        IRng NextRng,
        Size NextSize,
        string GenName,
        string Message)
            : GenIteration<T>(InitialRng, InitialSize, NextRng, NextSize);

    public sealed record GenDiscard<T>(
        IRng InitialRng,
        Size InitialSize,
        IRng NextRng,
        Size NextSize)
            : GenIteration<T>(InitialRng, InitialSize, NextRng, NextSize);

    public static class GenIterationExtensions
    {
        public static TResult Match<T, TResult>(
            this GenIteration<T> iteration,
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
