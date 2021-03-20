using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Runners;
using GalaxyCheck.Runners.Check;
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

    public static class PrintExtensions
    {
        public record Options<T>(Action<string>? StdOut, Func<T, string>? Format);

        private static string FormatDefault<T>(T value) => System.Text.Json.JsonSerializer.Serialize(value);

        public static void Print<T>(
            this Property<T> property,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {

            var options = new Options<T>(null, null);

            var stdOut = options.StdOut ?? Console.Write;
            Func<T, string> format = options.Format ?? FormatDefault;

            var result = property.Check(iterations: iterations, seed: seed, size: size);

            foreach (var checkIteration in result.Checks.OfType<CheckIteration.Check<T>>())
            {
                var iterationOutput = $@"
Testing value: {format(checkIteration.Value)} (seed = {checkIteration.Parameters.Rng.Seed}, size = {checkIteration.Parameters.Size.Value})
Result: {(checkIteration.IsCounterexample ? "falsifies property" : "does not falsify property")}
{string.Join("", Enumerable.Repeat("-", 50))}";

                stdOut(iterationOutput);
            }

            var finalOutput = result.Falsified
                ? @"Property was falsified"
                : "Property was not falsified";

            stdOut(Environment.NewLine + finalOutput);
        }
    }
}
