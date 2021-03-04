using GalaxyCheck.Internal.ExampleSpaces;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GalaxyCheck
{
    using GalaxyCheck.Runners.Sample;

    public static partial class Gen
    {
        public static T SampleOne<T>(
            this IGen<T> gen,
            int? seed = null,
            int? size = null) => gen.Sample(iterations: 1, seed: seed, size: size).Single();

        public static List<T> Sample<T>(
            this IGen<T> gen,
            int? iterations = null,
            int? seed = null,
            int? size = null) => gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size).Values;

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
            int? size = null) => SampleHelpers.RunSample(advanced, iterations: iterations, seed: seed, size: size);
    }
}

namespace GalaxyCheck.Runners.Sample
{
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
        public static SampleWithMetricsResult<IExampleSpace<T>> RunSample<T>(IGenAdvanced<T> advanced, int? iterations, int? seed, int? size)
        {
            var checkResult = new AdvancedToGen<T>(advanced).ForAll(x =>
            {
                if (x is Test test)
                {
                    try
                    {
                        test.Func(test.Input);
                    }
                    catch (Exception ex) when (ex is not Property.PropertyPreconditionException)
                    {
                    }
                }
            })
            .Check(iterations: iterations, seed: seed, size: size);

            var exampleSpaces = checkResult.Checks.OfType<CheckIteration.Check<T>>().Select(i => i.ExampleSpace).ToList();

            return new SampleWithMetricsResult<IExampleSpace<T>>(
                exampleSpaces,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }

        private class AdvancedToGen<T> : IGen<T>
        {
            public AdvancedToGen(IGenAdvanced<T> advanced)
            {
                Advanced = advanced;
            }

            public IGenAdvanced<T> Advanced { get; }

            IGenAdvanced IGen.Advanced => Advanced;
        }
    }
}