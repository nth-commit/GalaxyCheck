using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Runners.Check;
using GalaxyCheck.Runners.Replaying;
using System;
using System.Collections.Generic;
using GalaxyCheck.Runners.Check.Sizing;
using Automata = GalaxyCheck.Runners.Check.Automata;
using AutomataAsync = GalaxyCheck.Runners.Check.AutomataAsync;

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
                transitionAggregation.FinalContext.Shrinks + transitionAggregation.FinalContext.CompletedIterationsAfterCounterexample,
                transitionAggregation.FinalContext.Counterexample == null
                    ? null
                    : FromCounterexampleContext(transitionAggregation.FinalContext.Counterexample),
                transitionAggregation.Checks,
                initialParameters,
                transitionAggregation.FinalContext.NextParameters,
                transitionAggregation.TerminationReason);
        }

        public static async System.Threading.Tasks.Task<CheckResult<T>> CheckAsync<T>(
             this IGen<TestAsync<T>> property,
             int? iterations = null,
             int? seed = null,
             int? size = null,
             int? shrinkLimit = null,
             string? replay = null,
             bool deepCheck = true)
        {
            var resolvedIterations = iterations ?? 100;
            var (initialSize, resizeStrategy) = SizingAspectsAsync<T>.Resolve(size == null ? null : new Size(size.Value), resolvedIterations);

            var initialParameters = seed == null
                ? GenParameters.Create(initialSize)
                : GenParameters.Create(Rng.Create(seed.Value), initialSize);

            var initialContext = new AutomataAsync.CheckStateContext<T>(
                property,
                resolvedIterations,
                shrinkLimit ?? 500,
                initialParameters,
                deepCheck);

            AutomataAsync.CheckState<T> initialState = replay == null
                ? new AutomataAsync.GenerationStates.Generation_Begin<T>()
                : new AutomataAsync.ReplayState<T>(replay);

            var transitionAggregation = await AutomataAsync.CheckStateAggregator.Aggregate(
                initialState,
                initialContext,
                new[] { new ResizeCheckStateTransitionDecoratorAsync<T>(resizeStrategy) });

            return new CheckResult<T>(
                transitionAggregation.FinalContext.CompletedIterationsUntilCounterexample,
                transitionAggregation.FinalContext.Discards,
                transitionAggregation.FinalContext.Shrinks + transitionAggregation.FinalContext.CompletedIterationsAfterCounterexample,
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
                counterexampleContext.PresentationalValue);
        }

        private static Counterexample<T> FromCounterexampleContext<T>(
            AutomataAsync.CounterexampleContext<T> counterexampleContext)
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

        public IReadOnlyCollection<CheckIteration<T>> Checks { get; init; }

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
            IReadOnlyCollection<CheckIteration<T>> checks,
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
