using GalaxyCheck.Runners;
using GalaxyCheck.Runners.Sample;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Xunit
{
    public class SampleException : Exception
    {
        public SampleException(SampleWithMetricsResult<PresentableExampleSpace<Test<object>>> sample) : base(BuildMessage(sample)) { }

        private static string BuildMessage(SampleWithMetricsResult<PresentableExampleSpace<Test<object>>> sample) => string.Join(Environment.NewLine, BuildLines(sample));

        private static IEnumerable<string> BuildLines(SampleWithMetricsResult<PresentableExampleSpace<Test<object>>> sample)
        {
            const string LineBreak = "";

            yield return "Test case failed to prevent false-positives.";

            yield return LineBreak;

            yield return $"Sampled {sample.Values.Count} values ({sample.Discards} discarded):";

            for (var i = 0; i < sample.Values.Count; i++)
            {
                var value = sample.Values[i];

                var example = ExampleViewModel.Infer(
                    value.Presentational?.Current.Value ??
                    value.Actual.Current.Value.Input);

                var lines = ExampleRenderer.Render(example).ToList();

                yield return lines.Count switch
                {
                    0 => throw new Exception("Fatal: Expected at least one line to be rendered in counterexample"),
                    1 => $"Sample[{i}]: {lines.Single()}",
                    _ => string.Join(
                        Environment.NewLine,
                        Enumerable.Concat(
                            new[] { LineBreak, $"Sample[{i}]:" },
                            lines.Select(l => $"    {l}")))
                };
            }
        }
    }
}
