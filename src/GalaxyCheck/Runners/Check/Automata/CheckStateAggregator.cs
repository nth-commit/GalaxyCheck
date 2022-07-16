using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal record TransitionAggregation<T>(
        IReadOnlyCollection<CheckIteration<T>> Checks,
        CheckStateContext<T> FinalContext,
        TerminationReason TerminationReason);

    internal static class CheckStateAggregator
    {
        public static TransitionAggregation<T> Aggregate<T>(
            CheckState<T> initialState,
            CheckStateContext<T> initialContext,
            IReadOnlyCollection<ICheckStateTransitionDecorator<T>> decorators)
        {
            var transitions = Enumerate(initialState, initialContext, decorators);
            return AggregateTransitions(transitions);
        }

        private static IEnumerable<CheckStateTransition<T>> Enumerate<T>(
            CheckState<T> initialState,
            CheckStateContext<T> initialContext,
            IReadOnlyCollection<ICheckStateTransitionDecorator<T>> decorators) => EnumerableExtensions.Unfold(
                new CheckStateTransition<T>(initialState, initialContext),
                previousTransition =>
                {
                    if (previousTransition.State is TerminationState<T>)
                    {
                        return new Option.None<CheckStateTransition<T>>();
                    }

                    var transition = previousTransition.State.Transition(previousTransition.Context);
                    var decoratedTransition = DecorateTransition(decorators, previousTransition.State, transition);

                    return new Option.Some<CheckStateTransition<T>>(decoratedTransition);
                });

        private static CheckStateTransition<T> DecorateTransition<T>(
            IReadOnlyCollection<ICheckStateTransitionDecorator<T>> decorators,
            CheckState<T> previousState,
            CheckStateTransition<T> nextTransition) => decorators
                .Scan(
                    (previousState, nextTransition),
                    (acc, decorator) =>
                    {
                        var nextTransition = decorator.Decorate(acc.previousState, acc.nextTransition);
                        return (previousState: acc.nextTransition.State, nextTransition);
                    })
                .Select(x => x.nextTransition)
                .Last();

        private static TransitionAggregation<T> AggregateTransitions<T>(IEnumerable<CheckStateTransition<T>> transitions)
        {
            var mappedTransitions = transitions
                .WithDiscardCircuitBreaker(isTransitionCountedInConsecutiveDiscardCount, isTransitionDiscard)
                .Select(x => (
                    state: x.State,
                    check: MapStateToIterationOrIgnore(x.State),
                    context: x.Context))
                .ToImmutableList();

            var lastMappedTransition = mappedTransitions.Last();
            if (lastMappedTransition.state is not TerminationState<T> terminationState)
            {
                throw new Exception("Fatal: Check did not terminate");
            }

            return new TransitionAggregation<T>(
                mappedTransitions.Select(x => x.check).OfType<CheckIteration<T>>().ToList(),
                lastMappedTransition.context,
                terminationState.Reason);
        }
        private static bool isTransitionCountedInConsecutiveDiscardCount<T>(CheckStateTransition<T> transition) =>
            transition.State is GenerationStates.Generation_Discard<T> ||
            transition.State is InstanceExplorationStates.InstanceExploration_Counterexample<T> ||
            transition.State is InstanceExplorationStates.InstanceExploration_NonCounterexample<T> ||
            transition.State is InstanceExplorationStates.InstanceExploration_Discard<T>;

        private static bool isTransitionDiscard<T>(CheckStateTransition<T> transition) =>
            transition.State is GenerationStates.Generation_Discard<T> ||
            transition.State is InstanceExplorationStates.InstanceExploration_Discard<T>;

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
