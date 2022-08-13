using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal static class GenerationStates
    {
        internal record Generation_Begin<T> : CheckState<T>
        {
            public async Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context)
            {
                await Task.CompletedTask;

                if (context.CompletedIterations >= context.RequestedIterations)
                {
                    return new CheckStateTransition<T>(
                        new TerminationState<T>(TerminationReason.ReachedMaximumIterations),
                        context);
                }

                var iterations = context.Property.Advanced.Run(context.NextParameters);
                return new CheckStateTransition<T>(
                    new Generation_HoldingNextIteration<T>(iterations),
                    context);
            }
        }

        internal record Generation_HoldingNextIteration<T>(IEnumerable<IGenIteration<Property.AsyncTest<T>>> Iterations) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context)
            {
                var (head, tail) = Iterations;

                var state = head!.Match<CheckState<T>>(
                    onInstance: (instance) =>
                        new Generation_Instance<T>(instance),
                    onDiscard: (discard) =>
                        new Generation_Discard<T>(tail, discard),
                    onError: (error) =>
                        new Generation_Error<T>($"Error while running generator {error.GenName}: {error.Message}"));

                return Task.FromResult(new CheckStateTransition<T>(state, context));
            }
        }

        internal record Generation_Instance<T>(IGenInstance<Property.AsyncTest<T>> Iteration) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(
                new CheckStateTransition<T>(
                    new InstanceExplorationStates.InstanceExploration_Begin<T>(Iteration),
                    context));
        }

        internal record Generation_Discard<T>(
            IEnumerable<IGenIteration<Property.AsyncTest<T>>> NextIterations,
            IGenDiscard<Property.AsyncTest<T>> Iteration) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(
                new CheckStateTransition<T>(
                    new Generation_HoldingNextIteration<T>(NextIterations),
                    context.IncrementDiscards(wasLateDiscard: false)));
        }

        internal record Generation_Error<T>(string Description) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(
                new CheckStateTransition<T>(
                    new TerminationState<T>(TerminationReason.FoundError),
                    context));
        }

        internal record Generation_End<T>(
            IGenInstance<Property.AsyncTest<T>> Instance,
            CounterexampleContext<T>? CounterexampleContext,
            bool WasDiscard,
            bool WasLateDiscard,
            bool WasReplay) : CheckState<T>
        {
            public Task<CheckStateTransition<T>> Transition(CheckStateContext<T> context) => Task.FromResult(WasDiscard == true
                ? TransitionFromDiscard(context, Instance, WasLateDiscard)
                : CounterexampleContext == null
                    ? NextStateWithoutCounterexample(context, Instance)
                    : NextStateWithCounterexample(context, Instance, CounterexampleContext, WasReplay));

            private static CheckStateTransition<T> TransitionFromDiscard(
                CheckStateContext<T> context,
                IGenInstance<Property.AsyncTest<T>> instance,
                bool wasLateDiscard) => new CheckStateTransition<T>(
                    new Generation_Begin<T>(),
                    context
                        .WithNextGenParameters(instance.NextParameters)
                        .IncrementDiscards(wasLateDiscard));

            private static CheckStateTransition<T> NextStateWithoutCounterexample(
                CheckStateContext<T> context,
                IGenInstance<Property.AsyncTest<T>> instance)
            {
                var nextContext = context
                    .IncrementCompletedIterations()
                    .WithNextGenParameters(instance.NextParameters);

                return new CheckStateTransition<T>(new Generation_Begin<T>(), nextContext);
            }

            private static CheckStateTransition<T> NextStateWithCounterexample(
                CheckStateContext<T> context,
                IGenInstance<Property.AsyncTest<T>> instance,
                CounterexampleContext<T> counterexampleContext,
                bool wasReplay)
            {
                var nextContext = context
                    .IncrementCompletedIterations()
                    .WithNextGenParameters(instance.NextParameters)
                    .AddCounterexample(counterexampleContext);

                var terminationReason = TryTerminate(nextContext, counterexampleContext, instance, wasReplay);

                return terminationReason == null
                    ? new CheckStateTransition<T>(
                        new Generation_Begin<T>(),
                        nextContext)
                    : new CheckStateTransition<T>(
                        new TerminationState<T>(terminationReason.Value),
                        nextContext);
            }

            private static TerminationReason? TryTerminate(
                CheckStateContext<T> state,
                CounterexampleContext<T> counterexampleContext,
                IGenInstance<Property.AsyncTest<T>> instance,
                bool wasReplay)
            {
                if (wasReplay)
                {
                    // We just want to repeat the last failure. The check we are replaying presumably reached for the
                    // smallest counterexample anyway.
                    return TerminationReason.IsReplay;
                }

                if (!state.DeepCheck)
                {
                    return TerminationReason.DeepCheckDisabled;
                }

                if (state.CompletedIterations >= state.RequestedIterations)
                {
                    // We should stop reaching for smaller counterexamples if we are at max iterations already.
                    return TerminationReason.ReachedMaximumIterations;
                }

                if (state.Shrinks >= state.ShrinkLimit)
                {
                    return TerminationReason.ReachedShrinkLimit;
                }

                if (instance.ReplayParameters.Size.Value == 100)
                {
                    // We're at the max size, don't bother starting again from 0% as the compute time is most likely not
                    // worth it. TODO: Add config to disable this if someone REALLY wants to find a smaller counterexample.
                    return TerminationReason.ReachedMaximumSize;
                }

                if (counterexampleContext.ExampleSpace.Current.Distance == 0)
                {
                    // The counterexample is literally the smallest possible example that fits the constraints.
                    return TerminationReason.FoundTheoreticalSmallestCounterexample;
                }

                const int RecentCounterexampleThreshold = 3;
                var recentCounterexamples = state.CounterexampleContextHistory.Take(RecentCounterexampleThreshold).ToList();
                if (recentCounterexamples.Count() == RecentCounterexampleThreshold &&
                    recentCounterexamples.GroupBy(x => x.ExampleSpace.Current.Id).Count() == 1)
                {
                    // The recent counterexamples have settled somewhat, short-circuit the remaining iterations
                    return TerminationReason.FoundPragmaticSmallestCounterexample;
                }

                return null;
            }
        }
    }
}
