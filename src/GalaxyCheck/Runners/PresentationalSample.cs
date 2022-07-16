using GalaxyCheck.Properties;
using GalaxyCheck.Runners.Check;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    using GalaxyCheck.Runners.Sample;
    using System.Threading.Tasks;

    public static partial class Extensions
    {
        public static SampleWithMetricsResult<IReadOnlyList<object?>> SamplePresentationalWithMetrics(
            this Property property,
            int? iterations = null,
            int? seed = null,
            int? size = null) => PresentationalSampleHelpers.RunPresentationalValueSample(property, iterations: iterations, seed: seed, size: size);

        public static Task<SampleWithMetricsResult<IReadOnlyList<object?>>> SamplePresentationalWithMetricsAsync(
            this AsyncProperty property,
            int? iterations = null,
            int? seed = null,
            int? size = null) => PresentationalSampleHelpers.RunPresentationalValueSampleAsync(property, iterations: iterations, seed: seed, size: size);

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

            return ExtractSampleFromCheckResult(checkResult);
        }

        internal static async Task<SampleWithMetricsResult<IReadOnlyList<object?>>> RunPresentationalValueSampleAsync(
            AsyncProperty property,
            int? iterations,
            int? seed,
            int? size)
        {
            var propertyWithoutFailures = property
                .Where(TestMeetsPrecondition)
                .Select(MuteTestFailure);

            var checkResult = await propertyWithoutFailures.CheckAsync(iterations: iterations, seed: seed, size: size);

            return ExtractSampleFromCheckResult(checkResult);
        }

        internal static SampleWithMetricsResult<IReadOnlyList<object?>> RunPresentationalValueSample<T>(
            IGenAdvanced<T> advanced,
            int? iterations,
            int? seed,
            int? size)
        {
            var property = advanced.AsGen().ForAll(_ => true);

            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);

            return ExtractSampleFromCheckResult(checkResult);
        }

        private static bool TestMeetsPrecondition<T>(Test<T> test)
        {
            // Evaluate the test by accessing the lazily-evaluated `Output`
            return test.Output.Value.Result != TestResult.FailedPrecondition;
        }

        private static Test<T> MuteTestFailure<T>(Test<T> test)
        {
            
            return TestFactory.Create(test.Input, () => true, test.PresentedInput);
        }

        private static bool TestMeetsPrecondition<T>(AsyncTest<T> test)
        {
            // Evaluate the test by accessing the lazily-evaluated `Output`
            // TODO: Figure out how to make this async, though it's not important because it's likely mostly used during dev only.
            return (test.Output.Value.GetAwaiter().GetResult()).Result != TestResult.FailedPrecondition;
        }

        private static AsyncTest<T> MuteTestFailure<T>(AsyncTest<T> test)
        {
            // Create a test that always passes using the same input. We don't care if a property passes or fails when
            // we're sampling the input.
            return TestFactory.Create(test.Input, () => ValueTask.FromResult(true), test.PresentedInput);
        }

        private static SampleWithMetricsResult<IReadOnlyList<object?>> ExtractSampleFromCheckResult<T>(CheckResult<T> checkResult)
        {
            var values = checkResult.Checks
                .Select(check => check.PresentationalValue)
                .ToList();

            return new SampleWithMetricsResult<IReadOnlyList<object?>>(
                values,
                checkResult.Discards,
                checkResult.RandomnessConsumption);
        }
    }
}
