using System;
using System.Collections.Generic;
using System.Text.Json;
using static GalaxyCheck.SampleExtensions;

namespace GalaxyCheck.Xunit
{
    public class SampleException : Exception
    {
        public SampleException(SampleWithMetricsResult<object[]> sample) : base(BuildMessage(sample)) { }

        private static string BuildMessage(SampleWithMetricsResult<object[]> sample) => string.Join(Environment.NewLine, BuildLines(sample));

        private static IEnumerable<string> BuildLines(SampleWithMetricsResult<object[]> sample)
        {
            const string LineBreak = "";

            yield return "Test case failed to prevent false-positives.";

            yield return LineBreak;

            yield return $"Sampled {sample.Values.Count} values:";

            yield return LineBreak;

            foreach (var value in sample.Values)
            {
                yield return JsonSerializer.Serialize(value);
            }
        }
    }
}
