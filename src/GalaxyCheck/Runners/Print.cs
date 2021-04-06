using GalaxyCheck.Runners.Print;
using GalaxyCheck.Runners.Sample;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static void Print<T>(
            this IGenAdvanced<Test<T>> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            Action<string>? stdout = null)
        {
            var sample = advanced.SamplePresentableWithMetrics(
                iterations: iterations,
                seed: seed,
                size: size);

            PrintHelpers.Print(sample.Values, sample.Discards, stdout);
        }

        public static void Print<T>(
            this IGenAdvanced<T> advanced,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            Action<string>? stdout = null)
        {
            var sample = advanced.SamplePresentableWithMetrics(
                iterations: iterations,
                seed: seed,
                size: size);

            PrintHelpers.Print(sample.Values, sample.Discards, stdout);
        }
    }
}

namespace GalaxyCheck.Runners.Print
{
    internal static class PrintHelpers
    {
        internal static void Print<T>(
            IReadOnlyList<PresentableValue<T>> presentableValues,
            int discards,
            Action<string>? stdout)
        {
            stdout ??= Console.WriteLine;

            stdout($"Sampled {presentableValues.Count} values ({discards} discarded):");
            stdout("");

            for (var i = 0; i < presentableValues.Count; i++)
            {
                var presentableValue = presentableValues[i];
                var value = presentableValue.Presentational ?? presentableValue.Actual;
                var exampleViewModel = GetExampleViewModel(value!, presentableValue.Arity);

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
        private static ExampleViewModel GetExampleViewModel(object value, int arity)
        {
            if (arity == 0 && value is object[] arrValue && arrValue.Length == 0)
            {
                return new ExampleViewModel.Nullary();
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