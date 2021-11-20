using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using GalaxyCheck.Runners.Check;
using GalaxyCheck.Runners.CheckAutomata;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
            var (resolvedSize, resizeStrategy) = ResolveSizingAspects<T>(size, resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.Create(resolvedSize)
                : GenParameters.Create(Rng.Create(seed.Value), resolvedSize);

            var initialCtx = CheckStateContext<T>.Create(
                property,
                resolvedIterations,
                shrinkLimit ?? 500,
                initialParameters,
                resizeStrategy,
                deepCheck);

            CheckState<T> initialState = replay == null
                ? new GenerationStates.Generation_Begin<T>()
                : new ReplayState<T>(replay);

            var states = UnfoldStates(initialState, initialCtx);
            var stateAggregation = AggregateStates(states);

            return new CheckResult<T>(
                stateAggregation.FinalContext.CompletedIterations,
                stateAggregation.FinalContext.Discards,
                stateAggregation.FinalContext.Shrinks,
                stateAggregation.FinalContext.Counterexample == null
                    ? null
                    : FromCounterexampleContext(stateAggregation.FinalContext.Counterexample),
                stateAggregation.Checks,
                initialParameters,
                stateAggregation.FinalContext.NextParameters,
                stateAggregation.TerminationReason);
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

        private static IEnumerable<CheckStateTransition<T>> UnfoldStates<T>(CheckState<T> initialState, CheckStateContext<T> initialContext)
        {
            Func<CheckStateTransition<T>, bool> isStateCountedInConsecutiveDiscardCount = transition =>
                transition.NextState is GenerationStates.Generation_Discard<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_Counterexample<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_NonCounterexample<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_Discard<T>;

            Func<CheckStateTransition<T>, bool> isStateDiscard = transition =>
                transition.NextState is GenerationStates.Generation_Discard<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_Discard<T>;

            return EnumerableExtensions
                .Unfold(
                    new CheckStateTransition<T>(initialState, initialContext),
                    transition =>
                    {
                        if (transition.NextState is TerminationState<T>)
                        {
                            return new Option.None<CheckStateTransition<T>>();
                        }

                        var nextTransition = transition.NextState.Transition(transition.NextContext);
                        return new Option.Some<CheckStateTransition<T>>(nextTransition);
                    })
                .WithConsecutiveDiscardCount(
                    isStateCountedInConsecutiveDiscardCount,
                    isStateDiscard)
                .SelectMany(x =>
                {
                    var (transition, consecutiveDiscardCount) = x;
                    if (transition.NextState is GenerationStates.Generation_End<T> generationEndState && consecutiveDiscardCount >= 10)
                    {
                        var nextContext = transition.NextContext.WithNextGenParameters(generationEndState.Instance.NextParameters with
                        {
                            Size = generationEndState.Instance.NextParameters.Size.BigIncrement()
                        });

                        return UnfoldStates(transition.NextState, nextContext);
                    }

                    return new[] { transition };
                })
                .TakeWhileInclusive(state => state is not TerminationState<T>);
        }

        private record StateAggregation<T>(
            ImmutableList<CheckIteration<T>> Checks,
            CheckStateContext<T> FinalContext,
            TerminationReason TerminationReason);

        private static StateAggregation<T> AggregateStates<T>(IEnumerable<CheckStateTransition<T>> transitions)
        {
            Func<CheckStateTransition<T>, bool> isStateCountedInConsecutiveDiscardCount = transition =>
                transition.NextState is GenerationStates.Generation_Discard<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_Counterexample<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_NonCounterexample<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_Discard<T>;

            Func<CheckStateTransition<T>, bool> isStateDiscard = transition =>
                transition.NextState is GenerationStates.Generation_Discard<T> ||
                transition.NextState is InstanceExplorationStates.InstanceExploration_Discard<T>;

            // TODO: A lot of this can be simplified, now that transitions already have a context built-in
            var mappedTransitions = transitions
                .WithDiscardCircuitBreaker(isStateCountedInConsecutiveDiscardCount, isStateDiscard)
                .ScanInParallel<CheckStateTransition<T>, CheckStateContext<T>>(null!, (acc, curr) => curr.NextContext)
                .Select(x => (
                    state: x.element.NextState,
                    check: MapStateToIterationOrIgnore(x.element.NextState),
                    context: x.state))
                .ToImmutableList();

            var lastMappedTransition = mappedTransitions.Last();
            if (lastMappedTransition.state is not TerminationState<T> terminationState)
            {
                throw new Exception("Fatal: Check did not terminate");
            }

            return new StateAggregation<T>(
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
                    if (info.CounterexampleContext == null)
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
                if (info.CounterexampleContext != null)
                {
                    return SuperStrategicResize<T>()(info);
                }

                var size = sizes.Skip(info.CheckStateContext.CompletedIterations).FirstOrDefault() ?? new Size(0);
                return size;
            };
        }

        /// <summary>
        /// If you thought you were going to resize well have I got news for you. Resizing cancelled!
        /// </summary>
        private static ResizeStrategy<T> NoopResize<T>() => (info) => info.Iteration.NextParameters.Size;

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
