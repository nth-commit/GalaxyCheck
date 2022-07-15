﻿using GalaxyCheck.Properties;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    using GalaxyCheck.Runners.Sample;

    public static partial class Extensions
    {
        public static SampleWithMetricsResult<IReadOnlyList<object?>> SamplePresentationalWithMetrics(
            this Property property,
            int? iterations = null,
            int? seed = null,
            int? size = null) => PresentationalSampleHelpers.RunPresentationalValueSample(property, iterations: iterations, seed: seed, size: size);

        public static SampleWithMetricsResult<IReadOnlyList<object?>> SamplePresentationalWithMetrics<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null) => PresentationalSampleHelpers.RunPresentationalValueSample(advanced, iterations: iterations, seed: seed, size: size);
    }
}

namespace GalaxyCheck.Runners.Sample
{
    internal static class PresentationalSampleHelpers
    {
        internal static SampleWithMetricsResult<IReadOnlyList<object?>> RunPresentationalValueSample(
            Property property,
            int? iterations,
            int? seed,
            int? size)
        {
            var propertyWithoutFailures = property
                .Where(TestMeetsPrecondition)
                .Select(MuteTestFailure);

            var checkResult = propertyWithoutFailures.Check(iterations: iterations, seed: seed, size: size);

            var values = checkResult.Checks
                .Select(check => check.PresentationalValue)
                .ToList();

            return new SampleWithMetricsResult<IReadOnlyList<object?>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }

        internal static SampleWithMetricsResult<IReadOnlyList<object?>> RunPresentationalValueSample<T>(
            IGenAdvanced<T> advanced,
            int? iterations,
            int? seed,
            int? size)
        {
            var property = advanced.AsGen().ForAll(_ => true);

            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);

            var values = checkResult.Checks
                .Select(check => check.PresentationalValue)
                .ToList();

            return new SampleWithMetricsResult<IReadOnlyList<object?>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }

        private static bool TestMeetsPrecondition<T>(Test<T> test) =>
            test.Output.Value.Result != TestResult.FailedPrecondition;

        private static Test<T> MuteTestFailure<T>(Test<T> test) =>
            TestFactory.Create(test.Input, () => true, test.PresentedInput);
    }
}
