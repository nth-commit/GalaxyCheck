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

        public GenParameters InitialParameters { get; set; }

        public GenParameters NextParameters { get; set; }

        public bool Falsified => Counterexample != null;

        public int RandomnessConsumption => NextParameters.Rng.Order - InitialParameters.Rng.Order;

        public CheckResult(
            int iterations,
            int discards,
            Counterexample<T>? counterexample,
            ImmutableList<CheckIteration<T>> checks,
            GenParameters initialParameters,
            GenParameters nextParameters)
        {
            Iterations = iterations;
            Discards = discards;
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
        ImmutableList<int> RepeatPath,
        Exception? Exception)
            : Example<T>(Id, Value, Distance);

    public abstract record CheckIteration<T>;

    public static class CheckIteration
    {
        public record Check<T>(
            T Value,
            IExampleSpace<T> ExampleSpace,
            GenParameters Parameters,
            ImmutableList<int> Path,
            bool IsCounterexample,
            Exception? Exception) : CheckIteration<T>;

        public record Termination<T>(CheckExtensions.CheckContext<T> Context) : CheckIteration<T>;

        public record Discard<T> : CheckIteration<T>;
    }

    public static class CheckExtensions
    {
        public static CheckResult<T> Check<T>(
            this Property<T> property,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var initialParameters = new GenParameters(
                seed == null ? Rng.Spawn() : Rng.Create(seed.Value),
                size == null ? Size.MinValue : new Size(size.Value));

            ResizeStrategy<T> resizeStrategy = size == null ? SuperStrategicResize<T> : NoopResize<T>;

            var states = EnumerableExtensions
                .Unfold(
                    CheckState.Begin(property, iterations ?? 100, initialParameters, resizeStrategy),
                    prevState => prevState is CheckState.Termination<T>
                        ? new Option.None<CheckState<T>>()
                        : new Option.Some<CheckState<T>>(prevState.NextState()));

            var checks = states
                .Select(MapStateToIterationOrIgnore)
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
                termination.Context.Counterexample,
                checks,
                initialParameters,
                termination.Context.NextParameters);
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
                        return Size.MaxValue;
                    }

                    return nextSize;
                },
                onError: error => error.NextParameters.Size,
                onDiscard: discard => discard.NextParameters.Size);
        }

        private static Size NoopResize<T>(IGenIteration<Test<T>> lastIteration, bool wasCounterexample) =>
            lastIteration.NextParameters.Size;

        private static CheckIteration<T>? MapStateToIterationOrIgnore<T>(CheckState<T> state)
        {
            CheckIteration<T>? FromHandleCounterexample(CheckState.HandleCounterexampleExploration<T> state) =>
                new CheckIteration.Check<T>(
                    Value: state.CounterexampleExploration.ExampleSpace.Current.Value.Input,
                    ExampleSpace: state.CounterexampleExploration.ExampleSpace.Map(ex => ex.Input),
                    Parameters: state.Instance.RepeatParameters,
                    Path: state.CounterexampleExploration.Path.ToImmutableList(),
                    Exception: state.Counterexample?.Exception,
                    IsCounterexample: true);

            CheckIteration<T>? FromHandleNonCounterexample(CheckState.HandleNonCounterexampleExploration<T> state) =>
                new CheckIteration.Check<T>(
                    Value: state.NonCounterexampleExploration.ExampleSpace.Current.Value.Input,
                    ExampleSpace: state.NonCounterexampleExploration.ExampleSpace.Map(ex => ex.Input),
                    Parameters: state.Instance.RepeatParameters,
                    Path: state.NonCounterexampleExploration.Path.ToImmutableList(),
                    Exception: null,
                    IsCounterexample: false);

            CheckIteration<T>? FromTermination(CheckState.Termination<T> state) =>
                new CheckIteration.Termination<T>(state.Context);

            CheckIteration<T>? ToDiscard() =>
                new CheckIteration.Discard<T>();

            return state switch
            {
                CheckState.HandleCounterexampleExploration<T> s => FromHandleCounterexample(s),
                CheckState.HandleNonCounterexampleExploration<T> s => FromHandleNonCounterexample(s),
                CheckState.HandleDiscardExploration<T> s => ToDiscard(),
                CheckState.HandleDiscard<T> s => ToDiscard(),
                CheckState.HandleError<T> s =>
                    throw new Exceptions.GenErrorException(s.Error.GenName, s.Error.Message),
                CheckState.Termination<T> s => FromTermination(s),
                _ => null
            };
        }

        public abstract record CheckState<T>(CheckContext<T> Context)
        {
            internal abstract CheckState<T> NextState();
        }

        public delegate Size ResizeStrategy<T>(IGenIteration<Test<T>> lastIteration, bool wasCounterexample);

        public record CheckContext<T>(
            Property<T> Property,
            int RequestedIterations,
            int CompletedIterations,
            int Discards,
            ImmutableList<Counterexample<T>> CounterexampleHistory,
            GenParameters NextParameters,
            ResizeStrategy<T> ResizeStrategy)
        {
            internal static CheckContext<T> Create(
                Property<T> property,
                int requestedIterations,
                GenParameters initialParameters,
                ResizeStrategy<T> resizeStrategy) =>
                    new CheckContext<T>(
                        property,
                        requestedIterations,
                        0,
                        0,
                        ImmutableList.Create<Counterexample<T>>(),
                        initialParameters,
                        resizeStrategy);

            public Counterexample<T>? Counterexample => CounterexampleHistory
                .OrderBy(c => c.Distance)
                .ThenByDescending(c => c.RepeatParameters.Size.Value) // Bigger sizes are more likely to normalize
                .FirstOrDefault();

            internal CheckContext<T> IncrementCompletedIterations() => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations + 1,
                Discards,
                CounterexampleHistory,
                NextParameters,
                ResizeStrategy);

            internal CheckContext<T> IncrementDiscards() => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations,
                Discards + 1,
                CounterexampleHistory,
                NextParameters,
                ResizeStrategy);

            internal CheckContext<T> AddCounterexample(Counterexample<T> counterexample) => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations,
                Discards,
                Enumerable.Concat(new[] { counterexample }, CounterexampleHistory).ToImmutableList(),
                NextParameters,
                ResizeStrategy);

            internal CheckContext<T> WithNextGenParameters(GenParameters genParameters) => new CheckContext<T>(
                Property,
                RequestedIterations,
                CompletedIterations,
                Discards,
                CounterexampleHistory,
                genParameters,
                ResizeStrategy);
        }

        public static class CheckState
        {
            public static CheckState<T> Begin<T>(
                Property<T> property,
                int requestedIterations,
                GenParameters initialParameters,
                ResizeStrategy<T> resizeStrategy) =>
                    new Initial<T>(CheckContext<T>.Create(property, requestedIterations, initialParameters, resizeStrategy));

            public record Initial<T>(CheckContext<T> Context) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    if (Context.CompletedIterations >= Context.RequestedIterations)
                    {
                        return new Termination<T>(Context);
                    }

                    var iterations = Context.Property.Advanced.Run(Context.NextParameters);
                    return new HandleNextIteration<T>(Context, iterations);
                }
            }

            public record HandleNextIteration<T>(
                CheckContext<T> Context,
                IEnumerable<IGenIteration<Test<T>>> Iterations) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var (head, tail) = Iterations;
                    return head!.Match<CheckState<T>>(
                        onInstance: (instance) => new HandleInstanceExploration<T>(Context, instance),
                        onDiscard: (discard) => new HandleDiscard<T>(Context, tail, discard),
                        onError: (error) => new HandleError<T>(Context, error));
                }
            }

            public record HandleInstanceExploration<T>(
                CheckContext<T> Context,
                IGenInstance<Test<T>> Instance) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    AnalyzeExploration<Test<T>> analyze = (example) =>
                    {
                        try
                        {
                            var success = example.Value.Func(example.Value.Input);
                            return success ? ExplorationOutcome.Success() : ExplorationOutcome.Fail(null);
                        }
                        catch (Property.PropertyPreconditionException)
                        {
                            return ExplorationOutcome.Discard();
                        }
                        catch (Exception ex)
                        {
                            return ExplorationOutcome.Fail(ex);
                        }
                    };

                    var explorations = Instance.ExampleSpace.Explore(analyze);
                    return new HandleInstanceNextExplorationStage<T>(Context, Instance, explorations, null, true);
                }
            }

            public record HandleDiscard<T>(
                CheckContext<T> Context,
                IEnumerable<IGenIteration<Test<T>>> NextIterations,
                IGenDiscard<Test<T>> Discard) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var nextContext = Context.IncrementDiscards();
                    return new HandleNextIteration<T>(nextContext, NextIterations);
                }
            }

            public record HandleError<T>(
                CheckContext<T> Context,
                IGenError<Test<T>> Error) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new Termination<T>(Context);
            }

            public record HandleInstanceNextExplorationStage<T>(
                CheckContext<T> Context,
                IGenInstance<Test<T>> Instance,
                IEnumerable<ExplorationStage<Test<T>>> Explorations,
                Counterexample<T>? Counterexample,
                bool IsFirstExplorationStage) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState()
                {
                    var (head, tail) = Explorations;

                    if (head == null)
                    {
                        return new HandleInstanceComplete<T>(
                            Context,
                            Instance,
                            Counterexample,
                            WasDiscard: false);
                    }

                    return head.Match<CheckState<T>>(
                        onCounterexampleExploration: (counterexampleExploration) =>
                            new HandleCounterexampleExploration<T>(
                                Context,
                                Instance,
                                tail,
                                counterexampleExploration),
                        onNonCounterexampleExploration: (nonCounterexampleExploration) =>
                            new HandleNonCounterexampleExploration<T>(
                                Context,
                                Instance,
                                tail,
                                nonCounterexampleExploration,
                                Counterexample),
                        onDiscardExploration: () => IsFirstExplorationStage
                            ? new HandleInstanceComplete<T>(Context, Instance, Counterexample: null, WasDiscard: true)
                            : new HandleDiscardExploration<T>(Context, Instance, tail, Counterexample));
                }
            }

            public record HandleCounterexampleExploration<T>(
                CheckContext<T> Context,
                IGenInstance<Test<T>> Instance,
                IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
                ExplorationStage<Test<T>>.Counterexample CounterexampleExploration) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new HandleInstanceNextExplorationStage<T>(
                    Context,
                    Instance,
                    NextExplorations,
                    Counterexample,
                    false);

                public Counterexample<T> Counterexample
                {
                    get
                    {
                        var example = CounterexampleExploration.ExampleSpace.Current;
                        return new Counterexample<T>(
                            example.Id,
                            example.Value.Input,
                            example.Distance,
                            Instance.RepeatParameters,
                            CounterexampleExploration.Path.ToImmutableList(),
                            CounterexampleExploration.Exception);
                    }
                }
            }

            public record HandleNonCounterexampleExploration<T>(
                CheckContext<T> Context,
                IGenInstance<Test<T>> Instance,
                IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
                ExplorationStage<Test<T>>.NonCounterexample NonCounterexampleExploration,
                Counterexample<T>? PreviousCounterexample) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new HandleInstanceNextExplorationStage<T>(
                    Context,
                    Instance,
                    NextExplorations,
                    PreviousCounterexample,
                    false);
            }

            public record HandleDiscardExploration<T>(
                CheckContext<T> Context,
                IGenInstance<Test<T>> Instance,
                IEnumerable<ExplorationStage<Test<T>>> NextExplorations,
                Counterexample<T>? PreviousCounterexample) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => new HandleInstanceNextExplorationStage<T>(
                    Context,
                    Instance,
                    NextExplorations,
                    PreviousCounterexample,
                    false);
            }

            public record HandleInstanceComplete<T>(
                CheckContext<T> Context,
                IGenInstance<Test<T>> Instance,
                Counterexample<T>? Counterexample,
                bool WasDiscard) : CheckState<T>(Context)
            {
                internal override CheckState<T> NextState() => WasDiscard == true
                    ? NextStateOnDiscard(Context, Instance)
                    : Counterexample == null
                        ? NextStateWithoutCounterexample(Context, Instance)
                        : NextStateWithCounterexample(Context, Instance, Counterexample);

                private static CheckState<T> NextStateOnDiscard(
                    CheckContext<T> context,
                    IGenInstance<Test<T>> instance)
                {
                    // TODO: Resize on discards more like Gen.Where does
                    // TODO: Exhaustion protection
                    var nextRng = instance.NextParameters.Rng;
                    var nextSize = context.ResizeStrategy(instance, wasCounterexample: false);
                    return new Initial<T>(context
                        .IncrementDiscards()
                        .WithNextGenParameters(new GenParameters(instance.NextParameters.Rng, nextSize)));
                }

                private static CheckState<T> NextStateWithoutCounterexample(
                    CheckContext<T> context,
                    IGenInstance<Test<T>> instance)
                {
                    var nextRng = instance.NextParameters.Rng;
                    var nextSize = context.ResizeStrategy(instance, wasCounterexample: false);
                    return new Initial<T>(context
                        .IncrementCompletedIterations()
                        .WithNextGenParameters(new GenParameters(instance.NextParameters.Rng, nextSize)));
                }

                private static CheckState<T> NextStateWithCounterexample(
                    CheckContext<T> context,
                    IGenInstance<Test<T>> instance,
                    Counterexample<T> counterexample)
                {
                    var nextRng = instance.NextParameters.Rng;
                    var nextSize = context.ResizeStrategy(instance, wasCounterexample: true);
                    var nextContext = context
                        .IncrementCompletedIterations()
                        .WithNextGenParameters(new GenParameters(nextRng, nextSize))
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

                    if (instance.RepeatParameters.Size == Size.MaxValue)
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

                    return new Initial<T>(nextContext.WithNextGenParameters(new GenParameters(nextRng, nextSize)));
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
