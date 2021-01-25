using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Random;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.Utility;
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

        public Counterexample<T>? Counterexample { get; init; }

        public ImmutableList<CheckIteration<T>> Checks { get; init; }

        public IRng InitialRng { get; init; }

        public Size InitialSize { get; init; }

        public IRng NextRng { get; init; }

        public Size NextSize { get; init; }

        public bool Falsified => Counterexample != null;

        public int RandomnessConsumption => NextRng.Order - InitialRng.Order;

        public CheckResult(
            int iterations,
            int discards,
            Counterexample<T>? counterexample,
            ImmutableList<CheckIteration<T>> checks,
            IRng initialRng,
            Size initialSize,
            IRng nextRng,
            Size nextSize)
        {
            Iterations = iterations;
            Discards = discards;
            Checks = checks;
            Counterexample = counterexample;
            InitialRng = initialRng;
            InitialSize = initialSize;
            NextRng = nextRng;
            NextSize = nextSize;
        }
    }

    public record Counterexample<T>(
        ExampleId Id,
        T Value,
        decimal Distance,
        IRng RepeatRng,
        Size RepeatSize,
        ImmutableList<int> RepeatPath,
        Exception? Exception)
            : Example<T>(Id, Value, Distance);

    public abstract record CheckIteration<T>;

    public static class CheckIteration
    {
        public record Check<T>(
            T Value,
            IRng Rng,
            Size Size,
            ImmutableList<int> Path,
            bool IsCounterexample,
            Exception? Exception) : CheckIteration<T>;

        public record Termination<T>(CheckExtensions.CheckContext<T> Context) : CheckIteration<T>;

        public record Discard<T> : CheckIteration<T>;
    }

    public static class CheckExtensions
    {
        public static CheckResult<T> Check<T>(
            this IProperty<T> property,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var initialRng = seed == null ? Rng.Spawn() : Rng.Create(seed.Value);
            var initialSize = size == null ? Size.MinValue : new Size(size.Value);

            var states = EnumerableExtensions
                .Unfold(
                    CheckState.Begin<T>(property, iterations ?? 100, initialRng, initialSize),
                    prevState => prevState is CheckState.Termination<T>
                        ? new Option.None<CheckState<T>>()
                        : new Option.Some<CheckState<T>>(prevState.NextState()));

            var checks = states
                .Select(MapStateToIterationOrIgnore)
                .OfType<CheckIteration<T>>()
                .ToImmutableList();

            var finalCheckIteration = checks.Last();

            if (finalCheckIteration is not CheckIteration.Termination<T> termination)
            {
                throw new Exception("Fatal: Unrecognised state");
            }

            return new CheckResult<T>(
                termination.Context.CompletedIterations,
                termination.Context.Discards,
                termination.Context.Counterexample,
                checks,
                initialRng,
                initialSize,
                termination.Context.NextRng,
                termination.Context.NextSize);
        }

        private static CheckIteration<T>? MapStateToIterationOrIgnore<T>(CheckState<T> state)
        {
            CheckIteration<T>? FromHandleCounterexample(CheckState.HandleCounterexample<T> state) =>
                new CheckIteration.Check<T>(
                    Value: state.ExplorationStage.Example.Value.Input,
                    Rng: state.Instance.InitialRng,
                    Size: state.Instance.InitialSize,
                    Path: state.ExplorationStage.Path.ToImmutableList(),
                    Exception: state.Counterexample?.Exception,
                    IsCounterexample: true);

            CheckIteration<T>? FromHandleNonCounterexample(CheckState.HandleNonCounterexample<T> state) =>
                new CheckIteration.Check<T>(
                    Value: state.ExplorationStage.Example.Value.Input,
                    Rng: state.Instance.InitialRng,
                    Size: state.Instance.InitialSize,
                    Path: state.ExplorationStage.Path.ToImmutableList(),
                    Exception: null,
                    IsCounterexample: false);

            CheckIteration<T>? FromTermination(CheckState.Termination<T> state) =>
                new CheckIteration.Termination<T>(state.Context);

            CheckIteration<T>? FromHandleDiscard(CheckState.HandleDiscard<T> state) =>
                new CheckIteration.Discard<T>();

            return state switch
            {
                CheckState.HandleCounterexample<T> s => FromHandleCounterexample(s),
                CheckState.HandleNonCounterexample<T> s => FromHandleNonCounterexample(s),
                CheckState.HandleDiscard<T> s => FromHandleDiscard(s),
                CheckState.HandleError<T> s =>
                    throw new Exceptions.GenErrorException(s.Error.GenName, s.Error.Message),
                CheckState.HandleExhaustion<T> _ => 
                    throw new Exceptions.GenExhaustionException(),
                CheckState.Termination<T> s => FromTermination(s),
                _ => null
            };
        }

        public abstract record CheckState<T>(CheckContext<T> Context)
        {
            internal abstract CheckState<T> NextState();
        }

        public record CheckContext<T>(
            IProperty<T> Property,
            int RequestedIterations,
            int CompletedIterations,
            int Discards,
            ImmutableList<Counterexample<T>> CounterexampleHistory,
            IRng NextRng,
            Size NextSize)
        {
            internal static CheckContext<T> Create(
                IProperty<T> property,
                int requestedIterations,
                IRng initialRng,
                Size initialSize) =>
                    new CheckContext<T>(
                        property,
                        requestedIterations,
                        0,
                        0,
                        ImmutableList.Create<Counterexample<T>>(),
                        initialRng,
                        initialSize);

            public Counterexample<T>? Counterexample => CounterexampleHistory
                .OrderBy(c => c.Distance)
                .ThenByDescending(c => c.RepeatSize.Value) // Bigger sizes are more likely to normalize
                .FirstOrDefault();

            internal CheckContext<T> IncrementCompletedIterations() => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations + 1,
                Discards,
                CounterexampleHistory,
                NextRng,
                NextSize);

            internal CheckContext<T> IncrementDiscards() => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations,
                Discards + 1,
                CounterexampleHistory,
                NextRng,
                NextSize);

            internal CheckContext<T> AddCounterexample(Counterexample<T> counterexample) => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations,
                Discards,
                Enumerable.Concat(new[] { counterexample }, CounterexampleHistory).ToImmutableList(),
                NextRng,
                NextSize);

            internal CheckContext<T> WithNextRunValues(IRng nextRng, Size nextSize) => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations,
                Discards,
                CounterexampleHistory,
                nextRng,
                nextSize);
        }

        public static class CheckState
        {
            public static CheckState<T> Begin<T>(
                IProperty<T> property,
                int requestedIterations,
                IRng initialRng,
                Size initialSize) =>
                    new Initial<T>(CheckContext<T>.Create(property, requestedIterations, initialRng, initialSize));

            public record Initial<T>(CheckContext<T> Context) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var iterations = Context.Property.Advanced.Run(Context.NextRng, Context.NextSize);
                    return new HandleNextIteration<T>(Context, iterations);
                }
            }

            public record HandleNextIteration<T>(
                CheckContext<T> Context,
                IEnumerable<GenIteration<PropertyIteration<T>>> Iterations) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var (head, tail) = Iterations;
                    return head!.Match<PropertyIteration<T>, CheckState<T>>(
                        onInstance: (instance) => new HandleInstanceExploration<T>(Context, instance),
                        onDiscard: (discard) => new HandleDiscard<T>(Context, tail, discard),
                        onError: (error) => new HandleError<T>(Context, error));
                }
            }

            public record HandleInstanceExploration<T>(
                CheckContext<T> Context,
                GenInstance<PropertyIteration<T>> Instance) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var explorations = Instance.ExampleSpace.Explore(x => x.Func(x.Input));
                    return new HandleInstanceNextExplorationStage<T>(Context, Instance, explorations, null);
                }
            }

            public record HandleDiscard<T>(
                CheckContext<T> Context,
                IEnumerable<GenIteration<PropertyIteration<T>>> NextIterations,
                GenDiscard<PropertyIteration<T>> Discard) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var nextContext = Context.IncrementDiscards();
                    return nextContext.Discards > 100
                        ? new HandleExhaustion<T>(nextContext)
                        : new HandleNextIteration<T>(nextContext, NextIterations);
                }
            }

            public record HandleExhaustion<T>(CheckContext<T> Context) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new Termination<T>(Context);
            }

            public record HandleError<T>(
                CheckContext<T> Context,
                GenError<PropertyIteration<T>> Error) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new Termination<T>(Context);
            }

            public record HandleInstanceNextExplorationStage<T>(
                CheckContext<T> Context,
                GenInstance<PropertyIteration<T>> Instance,
                IEnumerable<ExplorationStage<PropertyIteration<T>>> Explorations,
                Counterexample<T>? Counterexample) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var (head, tail) = Explorations;

                    if (head == null)
                    {
                        return new HandleInstanceComplete<T>(
                            Context,
                            Instance,
                            Counterexample);
                    }

                    return head.IsCounterexample
                        ? new HandleCounterexample<T>(Context, Instance, tail, head, head.CounterexampleDetails!)
                        : new HandleNonCounterexample<T>(Context, Instance, tail, head, Counterexample);
                }
            }

            public record HandleCounterexample<T>(
                CheckContext<T> Context,
                GenInstance<PropertyIteration<T>> Instance,
                IEnumerable<ExplorationStage<PropertyIteration<T>>> NextExplorations,
                ExplorationStage<PropertyIteration<T>> ExplorationStage,
                CounterexampleDetails CounterexampleDetails) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new HandleInstanceNextExplorationStage<T>(
                    Context,
                    Instance,
                    NextExplorations,
                    Counterexample);

                public Counterexample<T> Counterexample => new Counterexample<T>(
                    ExplorationStage.Example.Id,
                    ExplorationStage.Example.Value.Input,
                    ExplorationStage.Example.Distance,
                    Instance.InitialRng,
                    Instance.InitialSize,
                    ExplorationStage.Path.ToImmutableList(),
                    CounterexampleDetails.Exception);
            }

            public record HandleNonCounterexample<T>(
                CheckContext<T> Context,
                GenInstance<PropertyIteration<T>> Instance,
                IEnumerable<ExplorationStage<PropertyIteration<T>>> NextExplorations,
                ExplorationStage<PropertyIteration<T>> ExplorationStage,
                Counterexample<T>? PreviousCounterexample) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new HandleInstanceNextExplorationStage<T>(
                    Context,
                    Instance,
                    NextExplorations,
                    PreviousCounterexample);
            }

            public record HandleInstanceComplete<T>(
                CheckContext<T> Context,
                GenInstance<PropertyIteration<T>> Instance,
                Counterexample<T>? Counterexample) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    if (Counterexample == null)
                    {
                        var nextContext = Context
                            .IncrementCompletedIterations()
                            .WithNextRunValues(Instance.NextRng, Instance.NextSize.Increment());

                        if (nextContext.CompletedIterations == nextContext.RequestedIterations)
                        {
                            return new Termination<T>(nextContext);
                        }

                        return new Initial<T>(nextContext);
                    }

                    return NextStateWithCounterexample(Context, Instance, Counterexample);
                }

                private static CheckState<T> NextStateWithCounterexample(
                    CheckContext<T> context,
                    GenInstance<PropertyIteration<T>> instance,
                    Counterexample<T> counterexample)
                {
                    var nextRng = instance.NextRng;
                    var candidateNextSize = instance.NextSize.BigIncrement();
                    var nextSize = candidateNextSize.Value < instance.NextSize.Value ? Size.MaxValue : candidateNextSize;

                    var nextContext = context
                        .IncrementCompletedIterations()
                        .WithNextRunValues(nextRng, nextSize)
                        .AddCounterexample(counterexample);

                    if (nextContext.CompletedIterations == nextContext.RequestedIterations)
                    {
                        return new Termination<T>(nextContext);
                    }

                    if (counterexample.Distance == 0)
                    {
                        // The counterexample is literally the smallest possible example. Can't get smaller than that.
                        return new Termination<T>(nextContext);
                    }

                    if (instance.InitialSize == Size.MaxValue)
                    {
                        // We're at the max size, don't bother starting again from 0% as the compute time is most
                        // likely not worth it. TODO: Add config to disable this if someone REALLY wants to find a
                        // smaller counterexample.
                        return new Termination<T>(nextContext);
                    }

                    const int RecentCounterexampleThreshold = 3;
                    var recentCounterexamples = nextContext.CounterexampleHistory.Take(RecentCounterexampleThreshold).ToList();
                    if (recentCounterexamples.Count() == RecentCounterexampleThreshold &&
                        recentCounterexamples.GroupBy(x => x.Id).Count() == 1)
                    {
                        // The recent counterexamples have settled somewhat, short-circuit the remaining iterations
                        return new Termination<T>(nextContext);
                    }

                    return new Initial<T>(nextContext.WithNextRunValues(nextRng, nextSize));
                }
            }

            public record Termination<T>(CheckContext<T> Context) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    throw new Exception("Fatal: Cannot transition from termination state");
                }
            }

        }
    }
}
