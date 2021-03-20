using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Runners;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static void Print<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            bool? enableLinqInference = null,
            Action<string>? stdout = null)
        {
            stdout ??= Console.WriteLine;

            var sample = advanced.SamplePresentableExampleSpaceWithMetrics(
                iterations: iterations,
                seed: seed,
                size: size,
                enableLinqInference: enableLinqInference);

            stdout($"Sampled {sample.Values.Count} values ({sample.Discards} discarded):");
            stdout("");

            for (var i = 0; i < sample.Values.Count; i++)
            {
                var exampleSpace = sample.Values[i].Presentational ?? sample.Values[i].Actual;
                var exampleViewModel = GetExampleViewModel(exampleSpace.Current);

                var lines = ExampleRenderer.Render(exampleViewModel).ToList();

                if (lines.Count == 1)
                {
                    stdout($"Sample[{i}]: {lines.Single()}");
                }
                else
                {
                    foreach (var line in FormatMultiaryLines(i, lines))
                    {
                        stdout(line);
                    }
                    stdout("");
                }
            }
        }

        /// <summary>
        /// Hacky. But it works for the test cases captured. Need to unify the arity model.
        /// </summary>
        private static ExampleViewModel GetExampleViewModel(IExample example)
        {
            var value = example.Value;

            if (example.Value is Test test)
            {
                return ExampleViewModel.Infer(test.Input);
            }

            if (example.Value is ExampleViewModel exampleViewModel)
            {
                return exampleViewModel switch
                {
                    ExampleViewModel.Nullary nullary => nullary,
                    ExampleViewModel.Unary unary => ExampleViewModel.Infer(unary.Value),
                    ExampleViewModel.Multiary multiary => ExampleViewModel.Infer(multiary.Values),
                    _ => throw new NotSupportedException()
                };
            }

            return ExampleViewModel.Infer(value);
        }

        private static IEnumerable<string> FormatMultiaryLines(int sampleIndex, List<string> lines)
        {
            yield return $"Sample[{sampleIndex}]:";

            foreach (var line in lines)
            {
                yield return $"    {line}";
            }
        }
    }
}
