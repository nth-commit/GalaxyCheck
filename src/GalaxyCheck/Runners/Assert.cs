using GalaxyCheck.Runners;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static void Assert<T>(
            this IProperty<T> property,
            int? iterations = null,
            int? seed = null,
            int? size = null)
        {
            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);
            if (checkResult.Falsified)
            {
                throw new PropertyFailedException<T>(checkResult.Counterexample!, checkResult.Iterations);
            }
        }
    }
}

namespace GalaxyCheck.Runners
{
    public class PropertyFailedException<T> : Exception
    {
        public PropertyFailedException(Counterexample<T> counterexample, int iterations)
            : base(BuildMessage(counterexample, iterations))
        {
        }

        private static string BuildMessage(Counterexample<T> counterexample, int iterations) =>
            string.Join(Environment.NewLine, BuildLines(counterexample, iterations));

        private static IEnumerable<string> BuildLines(Counterexample<T> counterexample, int iterations)
        {
            const string LineBreak = "";

            yield return LineBreak;

            yield return FalsifiedAfterLine(iterations);
            yield return ReproductionLine(counterexample);
            yield return CounterexampleValueLine(counterexample);

            yield return LineBreak;
            if (counterexample.Exception == null)
            {
                yield return "Property function returned false";
            }
            else
            {
                yield return ExceptionLine(counterexample.Exception);
            }
        }

        private static string FalsifiedAfterLine(int iterations) => iterations == 1 ? "Falsified after 1 test" : $"Falsified after {iterations} tests";

        private static string ReproductionLine(Counterexample<T> counterexample)
        {
            var pathFormatted = "new [] { }";

            var attributes =
                new List<(string name, string value)>
                {
                    ("Seed", counterexample.RepeatRng.Seed.ToString(CultureInfo.InvariantCulture)),
                    ("Size", counterexample.RepeatSize.Value.ToString(CultureInfo.InvariantCulture)),
                    ("Path", pathFormatted)
                }
                .Select(x => $"{x.name} = {x.value}");

            return $"Reproduction: ({string.Join(", ", attributes)})";
        }

        private static string CounterexampleValueLine(Counterexample<T> counterexample) =>
            $"Counterexample: {counterexample.Value}";

        private static string ExceptionLine(Exception ex) => $"---- {ex.Message}";
    }
}