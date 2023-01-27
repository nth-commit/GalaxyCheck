using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;

namespace GalaxyCheck.Gens.Internal.Iterations
{
    internal static class GenIterationFactory
    {
        private record GenIteration<T> : IGenIteration<T>
        {
            public GenIteration(
                GenParameters replayParameters,
                IGenData<T> data)
            {
                ReplayParameters = replayParameters;
                Data = data;
            }

            public GenParameters ReplayParameters { get; init; }

            public IGenData<T> Data { get; init; }

            IGenData IGenIteration.Data => Data.Match<IGenData>(
                onInstance: instanceData => GenData.InstanceData(instanceData.NextParameters, instanceData.ExampleSpace),
                onError: errorData => GenData.ErrorData(errorData.Message),
                onDiscard: discardData => GenData.DiscardData(discardData.NextParameters, discardData.ExampleSpace));

            public TResult Match<TResult>(
                Func<IGenInstance<T>, TResult> onInstance,
                Func<IGenError<T>, TResult> onError,
                Func<IGenDiscard<T>, TResult> onDiscard) => Data.Match(
                onInstance: _ => onInstance(new GenInstance<T>(ReplayParameters, Data)),
                onError: _ => onError(new GenError<T>(ReplayParameters, Data)),
                onDiscard: _ => onDiscard(new GenDiscard<T>(ReplayParameters, Data)));

            private record GenInstance<U> : GenIteration<U>, IGenInstance<U>
            {
                public GenInstance(
                    GenParameters replayParameters,
                    IGenData<U> data)
                    : base(replayParameters, data)
                {
                }

                public GenParameters NextParameters => Data.Instance!.NextParameters;

                public IExampleSpace<U> ExampleSpace => Data.Instance!.ExampleSpace;

                IExampleSpace IGenInstanceData.ExampleSpace => ExampleSpace;
            }

            private record GenError<U> : GenIteration<U>, IGenError<U>
            {
                public GenError(GenParameters replayParameters, IGenData<U> data)
                    : base(replayParameters, data)
                {
                }

                public string Message => Data.Error!.Message;
            }

            private record GenDiscard<U> : GenIteration<U>, IGenDiscard<U>
            {
                public GenDiscard(
                    GenParameters replayParameters,
                    IGenData<U> data)
                    : base(replayParameters, data)
                {
                }

                public GenParameters NextParameters => Data.Discard!.NextParameters;

                public IExampleSpace ExampleSpace => Data.Discard!.ExampleSpace;
            }
        }

        public static IGenIteration<T> Instance<T>(
            GenParameters replayParameters,
            GenParameters nextParameters,
            IExampleSpace<T> exampleSpace)
        {
            var instanceData = GenData<T>.InstanceData(nextParameters, exampleSpace);
            return new GenIteration<T>(replayParameters, instanceData);
        }

        public static IGenIteration<T> Error<T>(
            GenParameters replayParameters,
            string message) => new GenIteration<T>(
            replayParameters,
            GenData<T>.ErrorData(message));

        public static IGenIteration<T> Discard<T>(
            GenParameters replayParameters,
            GenParameters nextParameters,
            IExampleSpace exampleSpace) =>
            new GenIteration<T>(replayParameters, GenData<T>.DiscardData(nextParameters, exampleSpace));
    }
}
