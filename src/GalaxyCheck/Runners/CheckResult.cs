using System;
using System.Collections.Generic;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Parameters;

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
