using GalaxyCheck.Runners.Sample;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GalaxyCheck.Xunit
{
    public class SampleException : Exception
    {
        public SampleException(SampleWithMetricsResult<Test<object[]>> sample) : base(BuildMessage(sample)) { }

        private static string BuildMessage(SampleWithMetricsResult<Test<object[]>> sample) => string.Join(Environment.NewLine, BuildLines(sample));

        private static IEnumerable<string> BuildLines(SampleWithMetricsResult<Test<object[]>> sample)
        {
            const string LineBreak = "";

            yield return "Test case failed to prevent false-positives.";

            yield return LineBreak;

            yield return $"Sampled {sample.Values.Count} values ({sample.Discards} discarded):";

            yield return LineBreak;

            foreach (var value in sample.Values)
            {
                yield return JsonSerializer.Serialize(value.Input);
            }
        }
    }
}
