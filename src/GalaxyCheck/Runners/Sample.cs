using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Properties;
using GalaxyCheck.Runners.Check;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    using GalaxyCheck.Runners.Sample;

    public static partial class Extensions
    {
        public static T SampleOne<T>(
            this IGen<T> gen,
            int? seed = null,
            int? size = null) => gen.Sample(iterations: 1, seed: seed, size: size ?? 100).Single();

        public static List<T> Sample<T>(
            this IGen<T> gen,
            int? iterations = null,
            int? seed = null,
            int? size = null) => gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size).Values;

        public static SampleOneWithMetricsResult<T> SampleOneWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? seed = null,
            int? size = null)
        {
            var sample = advanced.SampleExampleSpacesWithMetrics(iterations: 1, seed: seed, size: size);
            return new SampleOneWithMetricsResult<T>(
                sample.Values.Single().Current.Value,
                sample.Discards,
                sample.RandomnessConsumption);
        }

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => advanced.SampleExampleSpacesWithMetrics(iterations: iterations, seed: seed, size: size).Select(ex => ex.Current.Value);

        public static IExampleSpace<T> SampleOneExampleSpace<T>(
            this IGenAdvanced<T> advanced,
            int? seed = null,
            int? size = null) => advanced.SampleExampleSpaces(iterations: 1, seed: seed, size: size).Single();

        public static List<IExampleSpace<T>> SampleExampleSpaces<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => advanced.SampleExampleSpacesWithMetrics(iterations: iterations, seed: seed, size: size).Values;

        public static SampleWithMetricsResult<IExampleSpace<T>> SampleExampleSpacesWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => SampleHelpers.RunExampleSpaceSample(advanced, iterations: iterations, seed: seed, size: size);
    }
}

namespace GalaxyCheck.Runners.Sample
{
    public record SampleOneWithMetricsResult<T>(
        T Value,
        int Discards,
        int RandomnessConsumption);

    public record SampleWithMetricsResult<T>(
        List<T> Values,
        int Discards,
        int RandomnessConsumption);

    internal static class SampleWithMetricsResultExtensions
    {
        public static SampleWithMetricsResult<TResult> Select<T, TResult>(
            this SampleWithMetricsResult<T> result,
            Func<T, TResult> selector) =>
                new SampleWithMetricsResult<TResult>(
                    result.Values.Select(selector).ToList(),
                    result.Discards,
                    result.RandomnessConsumption);
    }

    internal static class SampleHelpers
    {
        internal static SampleWithMetricsResult<IExampleSpace<T>> RunExampleSpaceSample<T>(
            IGenAdvanced<T> advanced,
            int? iterations,
            int? seed,
            int? size)
        {
            var property = advanced.AsGen().ForAll(_ => true);

            var checkResult = property.Check(iterations: iterations, seed: seed, size: size, deepCheck: false);

            var values = checkResult.Checks
                .Select(check => check.ExampleSpace)
                .ToList();

            return new SampleWithMetricsResult<IExampleSpace<T>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }
    }
}
