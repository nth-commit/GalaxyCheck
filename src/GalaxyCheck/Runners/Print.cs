using System;
using System.Linq;

namespace GalaxyCheck
{
    public static class PrintExtensions
    {
        public record Options<T>(Action<string>? StdOut, Func<T, string>? Format);

        private static string FormatDefault<T>(T value) => System.Text.Json.JsonSerializer.Serialize(value);

        public static void Print<T>(
            this IProperty<T> property,
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
