using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using GalaxyCheck.Runners.Check;
using GalaxyCheck.Runners.CheckAutomata;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GalaxyCheck.Gens.Parameters.Internal;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static CheckResult<T> Check<T>(
             this Property<T> property,
             int? iterations = null,
             int? seed = null,
             int? size = null,
             int? shrinkLimit = null,
             string? replay = null,
             bool deepCheck = true)
        {
            var resolvedIterations = iterations ?? 100;
            var (resolvedSize, resizeStrategy) = ResolveSizingAspects<T>(size, resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.Create(resolvedSize)
                : GenParameters.Create(Rng.Create(seed.Value), resolvedSize);

            var initialState = CheckState<T>.Create(
                property,
                resolvedIterations,
                shrinkLimit ?? 500,
                initialParameters,
                resizeStrategy,
                deepCheck);

            AbstractTransition<T> initialTransition = replay == null
                ? new InitialTransition<T>(initialState)
                : new ReplayTransition<T>(initialState, replay);

            var transitions = UnfoldTransitions(initialTransition);
            var transitionAggregation = AggregateTransitions(transitions);

            return new CheckResult<T>(
                transitionAggregation.FinalState.CompletedIterations,
                transitionAggregation.FinalState.Discards,
                transitionAggregation.FinalState.Shrinks,
                transitionAggregation.FinalState.Counterexample == null
                    ? null
                    : FromCounterexampleState(transitionAggregation.FinalState.Counterexample),
                transitionAggregation.Checks,
                initialParameters,
                transitionAggregation.FinalState.NextParameters,
                transitionAggregation.TerminationReason);
        }

        private static (Size initialSize, ResizeStrategy<T> resizeStrategy) ResolveSizingAspects<T>(int? givenSize, int resolvedIterations)
        {
            if (givenSize == null)
            {
                if (resolvedIterations >= 100)
                {
                    return (new Size(0), SuperStrategicResize<T>());
                }
                else
                {
                    var (initialSize, nextSizes) = SamplingSize.SampleSize(resolvedIterations);
                    return (initialSize!, PlannedResize<T>(nextSizes));
                }
            }

            return (new Size(givenSize.Value), NoopResize<T>());
        }

        private static IEnumerable<AbstractTransition<T>> UnfoldTransitions<T>(AbstractTransition<T> initialTransition) =>
            EnumerableExtensions.Unfold(
                initialTransition,
                previousTransition => previousTransition is Termination<T>
                    ? new Option.None<AbstractTransition<T>>()
                    : new Option.Some<AbstractTransition<T>>(previousTransition.NextTransition()));

        private record TransitionAggregation<T>(
            ImmutableList<CheckIteration<T>> Checks,
            CheckState<T> FinalState,
            TerminationReason TerminationReason);

        private static TransitionAggregation<T> AggregateTransitions<T>(IEnumerable<AbstractTransition<T>> transitions)
        {
            var mappedTransitions = transitions
                .ScanInParallel<AbstractTransition<T>, CheckState<T>>(null!, (acc, curr) => curr.State)
                .Select(x => (
                    transition: x.element,
                    check: MapTransitionToIterationOrIgnore(x.element),
                    state: x.state))
                .WithDiscardCircuitBreaker(x => x.check != null, x => x.check is CheckIteration.Discard<T>)
                .ToImmutableList();

            var lastMappedTransition = mappedTransitions.Last();
            if (lastMappedTransition.transition is not Termination<T> terminationTransition)
            {
                throw new Exception("Fatal: Check did not terminate");
            }

            return new TransitionAggregation<T>(
                mappedTransitions.Select(x => x.check).OfType<CheckIteration<T>>().ToImmutableList(),
                terminationTransition.State,
                terminationTransition.Reason);
        }

        private static Counterexample<T> FromCounterexampleState<T>(
            CounterexampleState<T> counterexampleState)
        {
            var replay = new Replay(counterexampleState.ReplayParameters, counterexampleState.ReplayPath);
            var replayEncoded = ReplayEncoding.Encode(replay);

            return new Counterexample<T>(
                counterexampleState.ExampleSpace.Current.Id,
                counterexampleState.ExampleSpace.Current.Value,
                counterexampleState.ExampleSpace.Current.Distance,
                counterexampleState.ReplayParameters,
                counterexampleState.ReplayPath,
                replayEncoded,
                counterexampleState.Exception,
                counterexampleState.EnableLinqInference
                    ? NavigateToPresentationalExample(counterexampleState.ExampleSpaceHistory, counterexampleState.ReplayPath)
                    : null);
        }

        /// <summary>
        /// When a resize is called for, do a small increment if the iteration wasn't a counterexample. This increment
        /// may loop back to zero. If it was a counterexample, no time to screw around. Activate turbo-mode and
        /// accelerate towards max size, and never loop back to zero. Because we know this is a fallible property, we 
        /// should generate bigger values, because bigger values are more likely to be able to normalize.
        /// </summary>
        private static ResizeStrategy<T> SuperStrategicResize<T>()
        {
            return (info) => info.Iteration.Match(
                onInstance: instance =>
                {
                    if (info.CounterexampleState == null)
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

        /// <summary>
        /// Resize according to the given size plan. If we find a counterexample, switch to super-strategic resizing.
        /// </summary>
        private static ResizeStrategy<T> PlannedResize<T>(IEnumerable<Size> plannedSizes)
        {
            var sizes = plannedSizes.ToImmutableList();
            return (info) =>
            {
                if (info.CounterexampleState != null)
                {
                    return SuperStrategicResize<T>()(info);
                }

                var size = sizes.Skip(info.CheckState.CompletedIterations).FirstOrDefault() ?? new Size(0);
                return size;
            };
        }

        /// <summary>
        /// If you thought you were going to resize well have I got news for you. Resizing cancelled!
        /// </summary>
        private static ResizeStrategy<T> NoopResize<T>() => (info) => info.Iteration.NextParameters.Size;

        private static CheckIteration<T>? MapTransitionToIterationOrIgnore<T>(AbstractTransition<T> transition)
        {
            CheckIteration<T>? FromHandleCounterexample(CounterexampleExplorationTransition<T> transition)
            {
                if (transition.CounterexampleState == null)
                {
                    throw new Exception("Fatal: Expected not null");
                }

                var exampleSpace = transition.CounterexampleState.ExampleSpace;

                var enablePresentationalInference = transition.CounterexampleExploration.ExampleSpace.Current.Value.EnableLinqInference;
                var presentationalExampleSpace = enablePresentationalInference
                    ? NavigateToPresentationalExampleSpace(transition.Instance.ExampleSpaceHistory, transition.CounterexampleExploration.Path)
                        ?.Cast<object>()
                        ?.Map(x => x == null ? null : UnwrapBinding(x))
                    : null;

                return new CheckIteration.Check<T>(
                    Value: exampleSpace.Current.Value,
                    PresentationalValue: presentationalExampleSpace?.Current.Value,
                    ExampleSpace: exampleSpace,
                    PresentationalExampleSpace: presentationalExampleSpace,
                    Parameters: transition.CounterexampleState.ReplayParameters,
                    Path: transition.CounterexampleState.ReplayPath,
                    Exception: transition.CounterexampleState.Exception,
                    IsCounterexample: true);
            }

            CheckIteration<T>? FromHandleNonCounterexample(NonCounterexampleExplorationTransition<T> transition)
            {
                var inputExampleSpace = transition.NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input);

                var exampleSpace = transition.NonCounterexampleExploration.ExampleSpace;

                var enablePresentationalInference = transition.NonCounterexampleExploration.ExampleSpace.Current.Value.EnableLinqInference;
                var presentationalExampleSpace = enablePresentationalInference
                    ? NavigateToPresentationalExampleSpace(transition.Instance.ExampleSpaceHistory, transition.NonCounterexampleExploration.Path)
                        ?.Cast<object>()
                        ?.Map(x => x == null ? null : UnwrapBinding(x))
                    : null;

                return new CheckIteration.Check<T>(
                    Value: exampleSpace.Current.Value.Input,
                    PresentationalValue: presentationalExampleSpace?.Current.Value,
                    ExampleSpace: inputExampleSpace,
                    PresentationalExampleSpace: presentationalExampleSpace,
                    Parameters: transition.Instance.ReplayParameters,
                    Path: transition.NonCounterexampleExploration.Path,
                    Exception: null,
                    IsCounterexample: false);
            }

            CheckIteration<T>? ToDiscard() =>
                new CheckIteration.Discard<T>();

            return transition switch
            {
                CounterexampleExplorationTransition<T> t => FromHandleCounterexample(t),
                NonCounterexampleExplorationTransition<T> t => FromHandleNonCounterexample(t),
                DiscardExplorationTransition<T> t => ToDiscard(),
                DiscardTransition<T> t => ToDiscard(),
                ErrorTransition<T> t => throw new Exceptions.GenErrorException(t.Error),
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
    }
}

namespace GalaxyCheck.Runners.Check
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

        public TerminationReason TerminationReason { get; set; }

        public bool Falsified => Counterexample != null;

        public int RandomnessConsumption => NextParameters.Rng.Order - InitialParameters.Rng.Order;

        public CheckResult(
            int iterations,
            int discards,
            int shrinks,
            Counterexample<T>? counterexample,
            ImmutableList<CheckIteration<T>> checks,
            GenParameters initialParameters,
            GenParameters nextParameters,
            TerminationReason terminationReason)
        {
            Iterations = iterations;
            Discards = discards;
            Shrinks = shrinks;
            Checks = checks;
            Counterexample = counterexample;
            InitialParameters = initialParameters;
            NextParameters = nextParameters;
            TerminationReason = terminationReason;
        }
    }

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

        public record Discard<T> : CheckIteration<T>;
    }

    public record Counterexample<T>(
        ExampleId Id,
        T Value,
        decimal Distance,
        GenParameters ReplayParameters,
        IEnumerable<int> ReplayPath,
        string Replay,
        Exception? Exception,
        object? PresentationalValue);

    public enum TerminationReason
    {
        IsReplay = 1,
        DeepCheckDisabled = 2,
        ReachedMaximumIterations = 3,
        ReachedMaximumSize = 4,
        FoundTheoreticalSmallestCounterexample = 5,
        FoundPragmaticSmallestCounterexample = 6,
        FoundError = 7
    }
}