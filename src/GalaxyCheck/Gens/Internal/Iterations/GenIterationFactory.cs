using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.Internal.Iterations
{
    internal static class GenIterationFactory
    {
        private record GenIteration<T> : IGenIteration<T>
        {
            public GenIteration(
                GenParameters replayParameters,
                GenParameters nextParameters,
                IGenData<T> data)
            {
                ReplayParameters = replayParameters;
                NextParameters = nextParameters;
                Data = data;
            }

            public GenParameters ReplayParameters { get; init; }

            public GenParameters NextParameters { get; init; }

            public IGenData<T> Data { get; init; }

            IGenData IGenIteration.Data => Data.Match<IGenData>(
                onInstance: instanceData => GenData.InstanceData(instanceData.ExampleSpace),
                onError: errorData => GenData.ErrorData(errorData.GenName, errorData.Message),
                onDiscard: discardData => GenData.DiscardData(discardData.ExampleSpace));

            public TResult Match<TResult>(
                Func<IGenInstance<T>, TResult> onInstance,
                Func<IGenError<T>, TResult> onError,
                Func<IGenDiscard<T>, TResult> onDiscard) => Data.Match(
                    onInstance: _ => onInstance(new GenInstance<T>(ReplayParameters, NextParameters, Data)),
                    onError: _ => onError(new GenError<T>(ReplayParameters, NextParameters, Data)),
                    onDiscard: _ => onDiscard(new GenDiscard<T>(ReplayParameters, NextParameters, Data)));

            private record GenInstance<U> : GenIteration<U>, IGenInstance<U>
            {
                public GenInstance(
                    GenParameters replayParameters,
                    GenParameters nextParameters,
                    IGenData<U> data)
                        : base(replayParameters, nextParameters, data)
                {
                }

                public IExampleSpace<U> ExampleSpace => Data.Instance!.ExampleSpace;

                IExampleSpace IGenInstanceData.ExampleSpace => ExampleSpace;
            }

            private record GenError<U> : GenIteration<U>, IGenError<U>
            {
                public GenError(
                    GenParameters replayParameters,
                    GenParameters nextParameters,
                    IGenData<U> data)
                        : base(replayParameters, nextParameters, data)
                {
                }

                public string GenName => Data.Error!.GenName;

                public string Message => Data.Error!.Message;
            }

            private record GenDiscard<U> : GenIteration<U>, IGenDiscard<U>
            {
                public GenDiscard(
                    GenParameters replayParameters,
                    GenParameters nextParameters,
                    IGenData<U> data)
                        : base(replayParameters, nextParameters, data)
                {
                }

                public IExampleSpace ExampleSpace => Data.Discard!.ExampleSpace;
            }
        }

        public static IGenIteration<T> Instance<T>(
            GenParameters replayParameters,
            GenParameters nextParameters,
            IExampleSpace<T> exampleSpace)
        {
            var instanceData = GenData<T>.InstanceData(exampleSpace);
            return new GenIteration<T>(replayParameters, nextParameters, instanceData);
        }

        public static IGenIteration<T> Error<T>(
            GenParameters replayParameters,
            GenParameters nextParameters,
            string genName,
            string message) => new GenIteration<T>(
                replayParameters,
                nextParameters,
                GenData<T>.ErrorData(genName, message));

        public static IGenIteration<T> Discard<T>(
            GenParameters replayParameters,
            GenParameters nextParameters,
            IExampleSpace exampleSpace) =>
                new GenIteration<T>(replayParameters, nextParameters, GenData<T>.DiscardData(exampleSpace));
    }
}
