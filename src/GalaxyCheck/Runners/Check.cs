using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using GalaxyCheck.Runners.Check;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using GalaxyCheck.Runners.Check.Automata;
using GalaxyCheck.Runners.Check.Sizing;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static CheckResult<T> Check<T>(
             this IGen<Test<T>> property,
             int? iterations = null,
             int? seed = null,
             int? size = null,
             int? shrinkLimit = null,
             string? replay = null,
             bool deepCheck = true)
        {
            var resolvedIterations = iterations ?? 100;
            var (initialSize, resizeStrategy) = SizingAspects<T>.Resolve(size == null ? null : new Size(size.Value), resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.Create(initialSize)
                : GenParameters.Create(Rng.Create(seed.Value), initialSize);

            var initialContext = new CheckStateContext<T>(
                property,
                resolvedIterations,
                shrinkLimit ?? 500,
                initialParameters,
                deepCheck);

            CheckState<T> initialState = replay == null
                ? new GenerationStates.Generation_Begin<T>()
                : new ReplayState<T>(replay);

            var transitions = CheckStateEnumerator.Enumerate(
                initialState,
                initialContext,
                new[] { new ResizeCheckStateTransitionDecorator<T>(resizeStrategy) });

            var transitionAggregation = AggregateTransitions(transitions);

            return new CheckResult<T>(
                transitionAggregation.FinalContext.CompletedIterations,
                transitionAggregation.FinalContext.Discards,
                transitionAggregation.FinalContext.Shrinks,
                transitionAggregation.FinalContext.Counterexample == null
                    ? null
                    : FromCounterexampleContext(transitionAggregation.FinalContext.Counterexample),
                transitionAggregation.Checks,
                initialParameters,
                transitionAggregation.FinalContext.NextParameters,
                transitionAggregation.TerminationReason);
        }

        private record TransitionAggregation<T>(
            ImmutableList<CheckIteration<T>> Checks,
            CheckStateContext<T> FinalContext,
            TerminationReason TerminationReason);

        private static TransitionAggregation<T> AggregateTransitions<T>(IEnumerable<CheckStateTransition<T>> transitions)
        {
            Func<CheckStateTransition<T>, bool> isTransitionCountedInConsecutiveDiscardCount = transition =>
                transition.State is GenerationStates.Generation_Discard<T> ||
                transition.State is InstanceExplorationStates.InstanceExploration_Counterexample<T> ||
                transition.State is InstanceExplorationStates.InstanceExploration_NonCounterexample<T> ||
                transition.State is InstanceExplorationStates.InstanceExploration_Discard<T>;

            Func<CheckStateTransition<T>, bool> isTransitionDiscard = transition =>
                transition.State is GenerationStates.Generation_Discard<T> ||
                transition.State is InstanceExplorationStates.InstanceExploration_Discard<T>;

            // TODO: A lot of this can be simplified, now that transitions already have a context built-in
            var mappedTransitions = transitions
                .WithDiscardCircuitBreaker(isTransitionCountedInConsecutiveDiscardCount, isTransitionDiscard)
                .ScanInParallel<CheckStateTransition<T>, CheckStateContext<T>>(null!, (acc, curr) => curr.Context)
                .Select(x => (
                    state: x.element.State,
                    check: MapStateToIterationOrIgnore(x.element.State),
                    context: x.state))
                .ToImmutableList();

            var lastMappedTransition = mappedTransitions.Last();
            if (lastMappedTransition.state is not TerminationState<T> terminationState)
            {
                throw new Exception("Fatal: Check did not terminate");
            }

            return new TransitionAggregation<T>(
                mappedTransitions.Select(x => x.check).OfType<CheckIteration<T>>().ToImmutableList(),
                lastMappedTransition.context,
                terminationState.Reason);
        }

        private static Counterexample<T> FromCounterexampleContext<T>(
            CounterexampleContext<T> counterexampleContext)
        {
            var replay = new Replay(counterexampleContext.ReplayParameters, counterexampleContext.ReplayPath);
            var replayEncoded = ReplayEncoding.Encode(replay);

            return new Counterexample<T>(
                counterexampleContext.ExampleSpace.Current.Id,
                counterexampleContext.ExampleSpace.Current.Value,
                counterexampleContext.ExampleSpace.Current.Distance,
                counterexampleContext.ReplayParameters,
                counterexampleContext.ReplayPath,
                replayEncoded,
                counterexampleContext.Exception,
                counterexampleContext.PresentationalValue);
        }

        private static CheckIteration<T>? MapStateToIterationOrIgnore<T>(CheckState<T> state)
        {
            CheckIteration<T>? FromHandleCounterexample(InstanceExplorationStates.InstanceExploration_Counterexample<T> state)
            {
                return new CheckIteration<T>(
                    Value: state.InputExampleSpace.Current.Value,
                    PresentationalValue:
                        state.TestExampleSpace.Current.Value.PresentedInput ??
                        PresentationInferrer.InferValue(state.ExampleSpaceHistory),
                    ExampleSpace: state.InputExampleSpace,
                    Parameters: state.CounterexampleContext.ReplayParameters,
                    Path: state.CounterexampleContext.ReplayPath,
                    Exception: state.CounterexampleContext.Exception,
                    IsCounterexample: true);
            }

            CheckIteration<T>? FromHandleNonCounterexample(InstanceExplorationStates.InstanceExploration_NonCounterexample<T> state)
            {
                return new CheckIteration<T>(
                    Value: state.InputExampleSpace.Current.Value,
                    PresentationalValue:
                        state.TestExampleSpace.Current.Value.PresentedInput ??
                        PresentationInferrer.InferValue(state.ExampleSpaceHistory),
                    ExampleSpace: state.InputExampleSpace,
                    Parameters: state.Instance.ReplayParameters,
                    Path: state.NonCounterexampleExploration.Path,
                    Exception: null,
                    IsCounterexample: false);
            }

            return state switch
            {
                InstanceExplorationStates.InstanceExploration_Counterexample<T> t => FromHandleCounterexample(t),
                InstanceExplorationStates.InstanceExploration_NonCounterexample<T> t => FromHandleNonCounterexample(t),
                GenerationStates.Generation_Error<T> t => throw new Exceptions.GenErrorException(t.Description),
                _ => null
            };
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

    public record CheckIteration<T>(
        T Value,
        object?[] PresentationalValue,
        IExampleSpace<T> ExampleSpace,
        GenParameters Parameters,
        IEnumerable<int> Path,
        bool IsCounterexample,
        Exception? Exception);

    public record Counterexample<T>(
        ExampleId Id,
        T Value,
        decimal Distance,
        GenParameters ReplayParameters,
        IEnumerable<int> ReplayPath,
        string Replay,
        Exception? Exception,
        object?[] PresentationalValue);

    public enum TerminationReason
    {
        IsReplay = 1,
        DeepCheckDisabled = 2,
        ReachedMaximumIterations = 3,
        ReachedMaximumSize = 4,
        FoundTheoreticalSmallestCounterexample = 5,
        FoundPragmaticSmallestCounterexample = 6,
        FoundError = 7,
        ReachedShrinkLimit = 8
    }
}
