using System;

namespace GalaxyCheck
{
    using GalaxyCheck.Runners;

    public static partial class Extensions
    {
        public static T Minimum<T>(
            this IGen<T> gen,
            int iterations = 100,
            int? seed = null,
            int? size = null,
            bool deepMinimum = false,
            Func<T, bool>? pred = null) => gen.Advanced.MinimumWithMetrics<T>(
                iterations: iterations,
                seed: seed,
                size: size,
                deepMinimum: deepMinimum,
                pred: pred).Value;

        public static MinimumWithMetrics<T> MinimumWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int iterations = 100,
            int? seed = null,
            int? size = null,
            bool deepMinimum = false,
            Func<T, bool>? pred = null)
        {
            pred ??= (_) => true;

            var property = advanced.AsGen().ForAll(x =>
            {
                var result = pred(x);
                return result == false;
            });

            var result = property.Check<T>(iterations: iterations, seed: seed, size: size, deepCheck: deepMinimum);

            if (result.Falsified)
            {
                return new MinimumWithMetrics<T>(result.Counterexample!.Value, result.Shrinks);
            }

            throw new Exceptions.NoMinimalFoundException();
        }
    }
}

namespace GalaxyCheck.Runners
{
    public record MinimumWithMetrics<T>(
        T Value,
        int Shrinks);
}
