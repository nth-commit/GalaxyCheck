using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Runners.Check;
using GalaxyCheck.Runners.Check.Sizing;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AsyncAutomata = GalaxyCheck.Runners.Check.AsyncAutomata;
using Automata = GalaxyCheck.Runners.Check.Automata;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static CheckResult<T> Check<T>(
            this IGen<Property.Test<T>> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            int? shrinkLimit = null,
            string? replay = null,
            bool deepCheck = true)
        {
            var resolvedIterations = iterations ?? 100;
            var (initialSize, resizeStrategy) =
                SizingAspects<T>.Resolve(size == null ? null : new Size(size.Value), resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.CreateRandom(initialSize)
                : GenParameters.Create(Rng.Create(seed.Value), initialSize);

            var initialContext = new Automata.CheckStateContext<T>(
                property,
                resolvedIterations,
                shrinkLimit ?? 500,
                initialParameters,
                deepCheck);

            Automata.CheckState<T> initialState = replay == null
                ? new Automata.GenerationStates.Generation_Begin<T>()
                : new Automata.ReplayState<T>(replay);

            var transitionAggregation = Automata.CheckStateAggregator.Aggregate(
                initialState,
                initialContext,
                new[] { new ResizeCheckStateTransitionDecorator<T>(resizeStrategy) });

            return new CheckResult<T>(
                transitionAggregation.FinalContext.CompletedIterationsUntilCounterexample,
                transitionAggregation.FinalContext.Discards,
                transitionAggregation.FinalContext.Shrinks +
                transitionAggregation.FinalContext.CompletedIterationsAfterCounterexample,
                transitionAggregation.FinalContext.Counterexample == null
                    ? null
                    : FromCounterexampleContext(transitionAggregation.FinalContext.Counterexample),
                transitionAggregation.Checks,
                initialParameters,
                transitionAggregation.FinalContext.NextParameters,
                transitionAggregation.TerminationReason);
        }

        public static async Task<CheckResult<T>> CheckAsync<T>(
            this IGen<Property.AsyncTest<T>> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            int? shrinkLimit = null,
            string? replay = null,
            bool deepCheck = true)
        {
            var resolvedIterations = iterations ?? 100;
            var (initialSize, resizeStrategy) =
                SizingAspectsAsync<T>.Resolve(size == null ? null : new Size(size.Value), resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.CreateRandom(initialSize)
                : GenParameters.Create(Rng.Create(seed.Value), initialSize);

            var initialContext = new AsyncAutomata.CheckStateContext<T>(
                property,
                resolvedIterations,
                shrinkLimit ?? 500,
                initialParameters,
                deepCheck);

            AsyncAutomata.CheckState<T> initialState = replay == null
                ? new AsyncAutomata.GenerationStates.Generation_Begin<T>()
                : new AsyncAutomata.ReplayState<T>(replay);

            var transitionAggregation = await AsyncAutomata.CheckStateAggregator.Aggregate(
                initialState,
                initialContext,
                new[] { new ResizeCheckStateTransitionDecoratorAsync<T>(resizeStrategy) });

            return new CheckResult<T>(
                transitionAggregation.FinalContext.CompletedIterationsUntilCounterexample,
                transitionAggregation.FinalContext.Discards,
                transitionAggregation.FinalContext.Shrinks +
                transitionAggregation.FinalContext.CompletedIterationsAfterCounterexample,
                transitionAggregation.FinalContext.Counterexample == null
                    ? null
                    : FromCounterexampleContext(transitionAggregation.FinalContext.Counterexample),
                transitionAggregation.Checks,
                initialParameters,
                transitionAggregation.FinalContext.NextParameters,
                transitionAggregation.TerminationReason);
        }

        private static Counterexample<T> FromCounterexampleContext<T>(
            Automata.CounterexampleContext<T> counterexampleContext)
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
                counterexampleContext.PresentedInput);
        }

        private static Counterexample<T> FromCounterexampleContext<T>(
            AsyncAutomata.CounterexampleContext<T> counterexampleContext)
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
                counterexampleContext.PresentedInput);
        }
    }
}

namespace GalaxyCheck.Runners.Check
{
    public record CheckResult<T>(
        int Iterations,
        int Discards,
        int Shrinks,
        Counterexample<T>? Counterexample,
        IReadOnlyCollection<CheckIteration<T>> Checks,
        GenParameters InitialParameters,
        GenParameters NextParameters,
        TerminationReason TerminationReason)
    {
        public TerminationReason TerminationReason { get; set; } = TerminationReason;

        public bool Falsified => Counterexample != null;

        public int RandomnessConsumption => NextParameters.Rng.Order - InitialParameters.Rng.Order;
    }

    public record CheckIteration<T>(
        IReadOnlyList<object?> PresentedInput,
        IExampleSpace<T> ExampleSpace,
        GenParameters Parameters,
        IEnumerable<int> Path,
        Exception? Exception)
    {
        public bool IsCounterexample => Exception != null;
    }

    public record Counterexample<T>(
        ExampleId Id,
        T Value,
        decimal Distance,
        GenParameters ReplayParameters,
        IEnumerable<int> ReplayPath,
        string Replay,
        Exception? Exception,
        IReadOnlyList<object?> PresentedInput);

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
