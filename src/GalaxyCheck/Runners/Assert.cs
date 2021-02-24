using GalaxyCheck.Runners;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static void Assert<T>(
            this IProperty<T> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            Func<object?, string>? formatValue = null,
            Func<(int seed, int size, IEnumerable<int> path), string>? formatReproduction = null)
        {
            var checkResult = property.Check(iterations: iterations, seed: seed, size: size);
            if (checkResult.Falsified)
            {
                throw new PropertyFailedException(
                    BoxCounterexample(checkResult.Counterexample!),
                    checkResult.Iterations,
                    formatValue,
                    formatReproduction);
            }
        }

        private static Counterexample<object?> BoxCounterexample<T>(Counterexample<T> counterexample)
        {
            var value = (object)counterexample.Value!;
            return new Counterexample<object?>(
                counterexample.Id,
                value,
                counterexample.Distance,
                counterexample.RepeatRng,
                counterexample.RepeatSize,
                counterexample.RepeatPath,
                counterexample.Exception);
        }

        public static void Assert(
            this MethodInfo methodInfo,
            object? target = null,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            Func<object[], string>? formatValue = null,
            Func<(int seed, int size, IEnumerable<int> path), string>? formatReproduction = null)
        {
            var gen = Parameters(methodInfo);

            var property = gen.ForAll(parameters =>
            {
                try
                {
                    methodInfo.Invoke(target, parameters);
                }
                catch (TargetInvocationException ex)
                {
                    throw ex.InnerException;
                }
            });

            Assert(
                property,
                iterations: iterations,
                seed: seed,
                size: size,
                formatValue: formatValue == null ? null : x => formatValue((object[])x),
                formatReproduction: formatReproduction);
        }
    }

}

namespace GalaxyCheck.Runners
{
    public class PropertyFailedException : Exception
    {
        public PropertyFailedException(
            Counterexample<object?> counterexample,
            int iterations,
            Func<object?, string>? formatValue,
            Func<(int seed, int size, IEnumerable<int> path), string>? formatReproduction)
            : base(BuildMessage(counterexample, iterations, formatValue, formatReproduction))
        {
        }

        private static string BuildMessage(
            Counterexample<object?> counterexample,
            int iterations,
            Func<object?, string>? formatValue,
            Func<(int seed, int size, IEnumerable<int> path), string>? formatReproduction) =>
                string.Join(Environment.NewLine, BuildLines(counterexample, iterations, formatValue, formatReproduction));

        private static IEnumerable<string> BuildLines(
            Counterexample<object?> counterexample,
            int iterations,
            Func<object?, string>? formatValue,
            Func<(int seed, int size, IEnumerable<int> path), string>? formatReproduction)
        {
            const string LineBreak = "";

            yield return LineBreak;

            yield return FalsifiedAfterLine(iterations);
            yield return ReproductionLine(counterexample, formatReproduction);
            yield return CounterexampleValueLine(counterexample, formatValue);

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

        private static string ReproductionLine(
            Counterexample<object?> counterexample,
            Func<(int seed, int size, IEnumerable<int> path), string>? formatReproduction)
        {
            static string PrefixLine(string reproductionFormatted) => $"Reproduction: {reproductionFormatted}";

            if (formatReproduction == null)
            {
                var attributes =
                    new List<(string name, string value)>
                    {
                        ("Seed", counterexample.RepeatRng.Seed.ToString(CultureInfo.InvariantCulture)),
                        ("Size", counterexample.RepeatSize.Value.ToString(CultureInfo.InvariantCulture)),
                    }
                    .Select(x => $"{x.name} = {x.value}");

                return PrefixLine($"({string.Join(", ", attributes)})");
            }
            else
            {
                var reproduction = (counterexample.RepeatRng.Seed, counterexample.RepeatSize.Value, counterexample.RepeatPath);
                return PrefixLine(formatReproduction(reproduction));
            }
        }

        private static string CounterexampleValueLine(
            Counterexample<object?> counterexample,
            Func<object?, string>? formatValue)
        {
            var formattedValue = formatValue?.Invoke(counterexample.Value) ?? counterexample.Value?.ToString(); 
            return $"Counterexample: {formattedValue}";
        }

        private static string ExceptionLine(Exception ex) => $"---- {ex.Message}";
    }
}