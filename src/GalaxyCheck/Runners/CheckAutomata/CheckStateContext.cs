﻿using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal record CheckStateContext<T>(
        IGen<Test<T>> Property,
        int RequestedIterations,
        int ShrinkLimit,
        int CompletedIterations,
        int Discards,
        int Shrinks,
        ImmutableList<CounterexampleContext<T>> CounterexampleContextHistory,
        GenParameters NextParameters,
        ResizeStrategy<T> ResizeStrategy,
        bool DeepCheck)
    {
        public static CheckStateContext<T> Create(
            IGen<Test<T>> property,
            int requestedIterations,
            int shrinkLimit,
            GenParameters initialParameters,
            ResizeStrategy<T> resizeStrategy,
            bool deepCheck) =>
                new CheckStateContext<T>(
                    property,
                    requestedIterations,
                    shrinkLimit,
                    0,
                    0,
                    0,
                    ImmutableList.Create<CounterexampleContext<T>>(),
                    initialParameters,
                    resizeStrategy,
                    deepCheck);

        public CounterexampleContext<T>? Counterexample => CounterexampleContextHistory
            .OrderBy(c => c.ExampleSpace.Current.Distance)
            .ThenByDescending(c => c.ReplayParameters.Size.Value) // Bigger sizes are more likely to normalize
            .FirstOrDefault();

        public CheckStateContext<T> IncrementCompletedIterations() => this with
        {
            CompletedIterations = CompletedIterations + 1
        };

        public CheckStateContext<T> IncrementDiscards() => this with
        {
            Discards = Discards + 1
        };

        public CheckStateContext<T> IncrementShrinks() => this with
        {
            Shrinks = Shrinks + 1
        };

        public CheckStateContext<T> AddCounterexample(CounterexampleContext<T> counterexampleContext) => this with
        {
            CounterexampleContextHistory = Enumerable
                .Concat(new[] { counterexampleContext }, CounterexampleContextHistory)
                .ToImmutableList()
        };

        public CheckStateContext<T> WithNextGenParameters(GenParameters genParameters) => this with
        {
            NextParameters = genParameters
        };
    }

    internal record ResizeStrategyInformation<T>(
        CheckStateContext<T> CheckStateContext,
        CounterexampleContext<T>? CounterexampleContext,
        IGenIteration<Test<T>> Iteration);

    internal delegate Size ResizeStrategy<T>(ResizeStrategyInformation<T> info);

    internal record CounterexampleContext<T>(
        IExampleSpace<Test<T>> TestExampleSpace,
        IEnumerable<Lazy<IExampleSpace<object>?>> ExampleSpaceHistory,
        GenParameters ReplayParameters,
        IEnumerable<int> ReplayPath,
        Exception? Exception)
    {
        public IExampleSpace<T> ExampleSpace => TestExampleSpace.Map(ex => ex.Input);

        public T Value => ExampleSpace.Current.Value;

        public object?[] PresentationalValue
        {
            get
            {
                var test = TestExampleSpace.Current.Value;
                var arity = test.PresentedInput?.Length;
                return arity > 0 ? test.PresentedInput! : PresentationInferrer.InferValue(ExampleSpaceHistory);
            }
        }
    }
}
