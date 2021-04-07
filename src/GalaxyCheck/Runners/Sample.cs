﻿using GalaxyCheck.ExampleSpaces;
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

        public static List<T> Sample<T>(
            this IGen<Test<T>> gen,
            int? iterations = null,
            int? seed = null,
            int? size = null) => gen.Advanced.SampleWithMetrics(iterations: iterations, seed: seed, size: size).Values;

        public static SampleOneWithMetricsResult<T> SampleOneWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? seed = null,
            int? size = null)
        {
            var sample = advanced.SamplePresentableWithMetrics(iterations: 1, seed: seed, size: size);
            return new SampleOneWithMetricsResult<T>(
                sample.Values.Single().Actual,
                sample.Discards,
                sample.RandomnessConsumption);
        }

        public static SampleWithMetricsResult<PresentableValue<T>> SamplePresentableWithMetrics<T>(
            this IGenAdvanced<Test<T>> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => SampleHelpers.RunPresentationalValueSample(advanced, iterations: iterations, seed: seed, size: size);

        public static SampleWithMetricsResult<PresentableValue<T>> SamplePresentableWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => SampleHelpers.RunPresentationalValueSample(advanced, iterations: iterations, seed: seed, size: size);

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => advanced.SampleExampleSpacesWithMetrics(iterations: iterations, seed: seed, size: size).Select(ex => ex.Current.Value);

        public static SampleWithMetricsResult<T> SampleWithMetrics<T>(
            this IGenAdvanced<Test<T>> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => advanced.SamplePresentableWithMetrics(iterations: iterations, seed: seed, size: size).Select(ex => ex.Actual);

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

        public static SampleWithMetricsResult<IExampleSpace<Test<T>>> SampleExampleSpacesWithMetrics<T>(
            this IGenAdvanced<Test<T>> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => SampleHelpers.RunExampleSpaceSample(advanced, iterations: iterations, seed: seed, size: size);
    }
}

namespace GalaxyCheck.Runners.Sample
{
    public record PresentableValue<T>(
        T Actual,
        object? Presentational,
        int Arity);

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
        internal static SampleWithMetricsResult<PresentableValue<T>> RunPresentationalValueSample<T>(
            IGenAdvanced<Test<T>> advanced,
            int? iterations,
            int? seed,
            int? size)
        {
            var property = advanced
                .AsGen()
                .Where(TestMeetsPrecondition)
                .Select(MuteTestFailure);

            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);

            var values = checkResult.Checks
                .OfType<CheckIteration.Check<T>>()
                .Select(check => new PresentableValue<T>(check.Value, check.PresentationalValue, check.Arity))
                .ToList();

            return new SampleWithMetricsResult<PresentableValue<T>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }

        internal static SampleWithMetricsResult<PresentableValue<T>> RunPresentationalValueSample<T>(
            IGenAdvanced<T> advanced,
            int? iterations,
            int? seed,
            int? size)
        {
            var property = advanced.AsGen().ForAll(_ => true);

            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);

            var values = checkResult.Checks
                .OfType<CheckIteration.Check<T>>()
                .Select(check => new PresentableValue<T>(check.Value, check.PresentationalValue, check.Arity))
                .ToList();

            return new SampleWithMetricsResult<PresentableValue<T>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }

        internal static SampleWithMetricsResult<IExampleSpace<T>> RunExampleSpaceSample<T>(
            IGenAdvanced<T> advanced,
            int? iterations,
            int? seed,
            int? size)
        {
            var property = advanced.AsGen().ForAll(_ => true);

            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);

            var values = checkResult.Checks
                .OfType<CheckIteration.Check<T>>()
                .Select(check => check.ExampleSpace)
                .ToList();

            return new SampleWithMetricsResult<IExampleSpace<T>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }

        private static bool TestMeetsPrecondition<T>(Test<T> test)
        {
            try
            {
                test.Func(test.Input);
            }
            catch (Exception ex) when (ex is Property.PropertyPreconditionException)
            {
                return false;
            }
            return true;
        }

        private static Test<T> MuteTestFailure<T>(Test<T> test) =>
            new TestImpl<T>((_) => true, test.Input, test.Present);
    }
}