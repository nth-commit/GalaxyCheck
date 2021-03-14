using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Random;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Utility;
using GalaxyCheck.Runners.CheckAutomata;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck
{
    public record CheckResult<T>
    {
        public int Iterations { get; init; }

        public int Discards { get; init; }

        public int Shrinks { get; init; }

        public Counterexample<T>? Counterexample { get; init; }

        public ImmutableList<CheckIteration<T>> Checks { get; init; }

        public GenParameters InitialParameters { get; set; }

        public GenParameters NextParameters { get; set; }

        public bool Falsified => Counterexample != null;

        public int RandomnessConsumption => NextParameters.Rng.Order - InitialParameters.Rng.Order;

        public CheckResult(
            int iterations,
            int discards,
            int shrinks,
            Counterexample<T>? counterexample,
            ImmutableList<CheckIteration<T>> checks,
            GenParameters initialParameters,
            GenParameters nextParameters)
        {
            Iterations = iterations;
            Discards = discards;
            Shrinks = shrinks;
            Checks = checks;
            Counterexample = counterexample;
            InitialParameters = initialParameters;
            NextParameters = nextParameters;
        }
    }

    public record Counterexample<T>(
        ExampleId Id,
        T Value,
        decimal Distance,
        GenParameters RepeatParameters,
        IEnumerable<int> RepeatPath,
        Exception? Exception,
        object? PresentationalValue);

    public abstract record CheckIteration<T>;

    public static class CheckIteration
    {
        public record Check<T>(
            T Value,
            object? PresentationalValue,
            IExampleSpace<T> ExampleSpace,
            IExampleSpace? PresentationalExampleSpace,
            GenParameters Parameters,
            IEnumerable<int> Path,
            bool IsCounterexample,
            Exception? Exception) : CheckIteration<T>;

        public record Termination<T>(CheckState<T> Context) : CheckIteration<T>;

        public record Discard<T> : CheckIteration<T>;
    }

    public static class CheckExtensions
    {
        public static CheckResult<T> Check<T>(
             this Property<T> property,
             int? iterations = null,
             int? seed = null,
             int? size = null,
             int? shrinkLimit = null,
             string? replay = null)
        {
            var initialParameters = new GenParameters(
                Rng: seed == null ? Rng.Spawn() : Rng.Create(seed.Value),
                Size: new Size(size ?? 0));

            ResizeStrategy<T> resizeStrategy = size == null ? SuperStrategicResize<T> : NoopResize<T>;

            var initialState = CheckState<T>.Create(
                property,
                iterations ?? 100,
                shrinkLimit ?? 500,
                initialParameters,
                resizeStrategy);

            AbstractTransition<T> initialTransition = replay == null
                ? new InitialTransition<T>(initialState)
                : new ReplayTransition<T>(initialState, replay);

            var transitions = EnumerableExtensions
                .Unfold(
                    initialTransition,
                    previousTransition => previousTransition is Termination<T>
                        ? new Option.None<AbstractTransition<T>>()
                        : new Option.Some<AbstractTransition<T>>(previousTransition.NextTransition()));

            var checks = transitions
                .Select(check => MapStateToIterationOrIgnore(check, property.Options.EnableLinqInference))
                .OfType<CheckIteration<T>>()
                .WithDiscardCircuitBreaker(state => state is CheckIteration.Discard<T>)
                .ToImmutableList();

            var finalCheckIteration = checks.Last();

            if (finalCheckIteration is not CheckIteration.Termination<T> termination)
            {
                throw new Exception("Fatal: Unrecognised state");
            }

            return new CheckResult<T>(
                termination.Context.CompletedIterations,
                termination.Context.Discards,
                termination.Context.Shrinks,
                termination.Context.Counterexample == null
                    ? null
                    : FromCounterexampleState(termination.Context.Counterexample, property.Options.EnableLinqInference),
                checks,
                initialParameters,
                termination.Context.NextParameters);
        }

        private static Counterexample<T> FromCounterexampleState<T>(
            CounterexampleState<T> counterexampleState,
            bool enablePresentationalInference)
        {
            return new Counterexample<T>(
                counterexampleState.ExampleSpace.Current.Id,
                counterexampleState.ExampleSpace.Current.Value,
                counterexampleState.ExampleSpace.Current.Distance,
                counterexampleState.RepeatParameters,
                counterexampleState.RepeatPath,
                counterexampleState.Exception,
                enablePresentationalInference
                    ? NavigateToPresentationalExample(counterexampleState.ExampleSpaceHistory, counterexampleState.RepeatPath)
                    : null);
        }

        /// <summary>
        /// When a resize is called for, do a small increment if the iteration wasn't a counterexample. This increment
        /// may loop back to zero. If it was a counterexample, no time to screw around. Activate turbo-mode and
        /// accelerate towards max size, and never loop back to zero. Because we know this is a fallible property, we 
        /// should generate bigger values, because bigger values are more likely to be able to normalize.
        /// </summary>
        /// <returns></returns>
        private static Size SuperStrategicResize<T>(IGenIteration<Test<T>> lastIteration, bool wasCounterexample)
        {
            return lastIteration.Match(
                onInstance: instance =>
                {
                    if (wasCounterexample == false)
                    {
                        return instance.NextParameters.Size.Increment();
                    }

                    var nextSize = instance.NextParameters.Size.BigIncrement();

                    var incrementWrappedToZero = nextSize.Value < instance.NextParameters.Size.Value;
                    if (incrementWrappedToZero)
                    {
                        // Clamp to MaxValue
                        return new Size(100);
                    }

                    return nextSize;
                },
                onError: error => error.NextParameters.Size,
                onDiscard: discard => discard.NextParameters.Size);
        }

        private static Size NoopResize<T>(IGenIteration<Test<T>> lastIteration, bool wasCounterexample) =>
            lastIteration.NextParameters.Size;

        private static CheckIteration<T>? MapStateToIterationOrIgnore<T>(AbstractTransition<T> state, bool enablePresentationalInference)
        {
            CheckIteration<T>? FromHandleCounterexample(CounterexampleExplorationTransition<T> state, bool enablePresentationalInference)
            {
                if (state.CounterexampleState == null)
                {
                    throw new Exception("Fatal: Expected not null");
                }

                var exampleSpace = state.CounterexampleState.ExampleSpace;

                var presentationalExampleSpace = enablePresentationalInference
                    ? NavigateToPresentationalExampleSpace(state.Instance.ExampleSpaceHistory, state.CounterexampleExploration.Path)
                        ?.Cast<object>()
                        ?.Map(x => x == null ? null : UnwrapBinding(x))
                    : null;

                return new CheckIteration.Check<T>(
                    Value: exampleSpace.Current.Value,
                    PresentationalValue: presentationalExampleSpace?.Current.Value,
                    ExampleSpace: exampleSpace,
                    PresentationalExampleSpace: presentationalExampleSpace,
                    Parameters: state.CounterexampleState.RepeatParameters,
                    Path: state.CounterexampleState.RepeatPath,
                    Exception: state.CounterexampleState.Exception,
                    IsCounterexample: true);
            }

            CheckIteration<T>? FromHandleNonCounterexample(NonCounterexampleExplorationTransition<T> state, bool enablePresentationalInference)
            {
                var inputExampleSpace = state.NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

                var exampleSpace = state.NonCounterexampleExploration.ExampleSpace;

                var presentationalExampleSpace = enablePresentationalInference
                    ? NavigateToPresentationalExampleSpace(state.Instance.ExampleSpaceHistory, state.NonCounterexampleExploration.Path)
                        ?.Cast<object>()
                        ?.Map(x => x == null ? null : UnwrapBinding(x))
                    : null;

                return new CheckIteration.Check<T>(
                    Value: exampleSpace.Current.Value.Input,
                    PresentationalValue: presentationalExampleSpace?.Current.Value,
                    ExampleSpace: inputExampleSpace,
                    PresentationalExampleSpace: presentationalExampleSpace,
                    Parameters: state.Instance.RepeatParameters,
                    Path: state.NonCounterexampleExploration.Path,
                    Exception: null,
                    IsCounterexample: false);
            }

            CheckIteration<T>? FromTermination(Termination<T> state) =>
                new CheckIteration.Termination<T>(state.State);

            CheckIteration<T>? ToDiscard() =>
                new CheckIteration.Discard<T>();

            return state switch
            {
                CounterexampleExplorationTransition<T> s => FromHandleCounterexample(s, enablePresentationalInference),
                NonCounterexampleExplorationTransition<T> s => FromHandleNonCounterexample(s, enablePresentationalInference),
                DiscardExplorationTransition<T> s => ToDiscard(),
                DiscardTransition<T> s => ToDiscard(),
                ErrorTransition<T> s =>
                    throw new Exceptions.GenErrorException(s.Error.GenName, s.Error.Message),
                Termination<T> s => FromTermination(s),
                _ => null
            };
        }

        private static object? NavigateToPresentationalExample(
            IEnumerable<IExampleSpace> exampleSpaceHistory,
            IEnumerable<int> navigationPath)
        {
            var value = NavigateToPresentationalExampleSpace(exampleSpaceHistory, navigationPath)?.Current.Value;
            if (value == null)
            {
                return null;
            }

            return UnwrapBinding(value);
        }

        private static IExampleSpace? NavigateToPresentationalExampleSpace(
            IEnumerable<IExampleSpace> exampleSpaceHistory,
            IEnumerable<int> navigationPath)
        {
            var rootExampleSpace = FindPresentationalExampleSpace(exampleSpaceHistory);
            if (rootExampleSpace == null)
            {
                return null;
            }

            return rootExampleSpace.Cast<object>().Navigate(navigationPath);
        }

        private static IExampleSpace? FindPresentationalExampleSpace(IEnumerable<IExampleSpace> exampleSpaceHistory)
        {
            var (head, tail) = exampleSpaceHistory.Reverse();

            var presentationalExampleSpaces =
                from exs in tail
                where exs.Current.Id.HashCode == head!.Current.Id.HashCode
                where
                    exs.Current.Value is not Test test ||
                    test.Arity != 0
                select exs;

            return presentationalExampleSpaces.FirstOrDefault();
        }

        private static object? UnwrapBinding(object obj)
        {
            static object?[] UnwrapBindingRec(object? obj)
            {
                var type = obj?.GetType();

                if (type != null && type.IsAnonymousType())
                {
                    return type.GetProperties().SelectMany(p => UnwrapBindingRec(p.GetValue(obj))).ToArray();
                }

                return new[] { obj };
            }

            if (obj?.GetType().IsAnonymousType() == true)
            {
                return UnwrapBindingRec(obj);
            }

            return obj;
        }

        public delegate Size ResizeStrategy<T>(IGenIteration<Test<T>> lastIteration, bool wasCounterexample);

        public record CounterexampleState<T>(
            IExampleSpace<T> ExampleSpace,
            IEnumerable<IExampleSpace> ExampleSpaceHistory,
            GenParameters RepeatParameters,
            IEnumerable<int> RepeatPath,
            Exception? Exception);
    }
}
