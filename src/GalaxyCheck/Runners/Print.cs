using GalaxyCheck.Runners.Print;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static void Print(
            this Property property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            Action<string>? stdout = null)
        {
            var sample = property.SamplePresentationalWithMetrics(
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
            var sample = advanced.SamplePresentationalWithMetrics(
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
        internal static void Print(
            IReadOnlyList<IReadOnlyList<object?>> presentationalValues,
            int discards,
            Action<string>? stdout)
        {
            stdout ??= Console.WriteLine;

            stdout($"Sampled {presentationalValues.Count} values ({discards} discarded):");
            stdout("");

            for (var i = 0; i < presentationalValues.Count; i++)
            {
                var presentationalValue = presentationalValues[i];
                var lines = ExampleRenderer.Render(presentationalValue).ToList();

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
