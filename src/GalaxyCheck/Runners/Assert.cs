using GalaxyCheck.Runners;
using GalaxyCheck.Runners.Check;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static void Assert<T>(
            this IGen<Test<T>> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            int? shrinkLimit = null,
            string? replay = null,
            bool deepCheck = true,
            Func<string, string>? formatReproduction = null,
            Func<string, string>? formatMessage = null)
        {
            var checkResult = property.Check(iterations: iterations, seed: seed, size: size, shrinkLimit: shrinkLimit, replay: replay, deepCheck: deepCheck);

            if (checkResult.Falsified)
            {
                throw new PropertyFailedException(
                    BoxCounterexample(checkResult.Counterexample!),
                    checkResult.Iterations,
                    checkResult.Shrinks,
                    formatReproduction,
                    formatMessage);
            }
        }

        public static async Task AssertAsync<T>(
            this IGen<TestAsync<T>> property,
            int? iterations = null,
            int? seed = null,
            int? size = null,
            int? shrinkLimit = null,
            string? replay = null,
            bool deepCheck = true,
            Func<string, string>? formatReproduction = null,
            Func<string, string>? formatMessage = null)
        {
            var checkResult = await property.CheckAsync(iterations: iterations, seed: seed, size: size, shrinkLimit: shrinkLimit, replay: replay, deepCheck: deepCheck);

            if (checkResult.Falsified)
            {
                throw new PropertyFailedException(
                    BoxCounterexample(checkResult.Counterexample!),
                    checkResult.Iterations,
                    checkResult.Shrinks,
                    formatReproduction,
                    formatMessage);
            }
        }

        private static Counterexample<object?> BoxCounterexample<T>(Counterexample<T> counterexample)
        {
            var value = (object)counterexample.Value!;
            return new Counterexample<object?>(
                counterexample.Id,
                value,
                counterexample.Distance,
                counterexample.ReplayParameters,
                counterexample.ReplayPath,
                counterexample.Replay,
                counterexample.Exception,
                counterexample.PresentationalValue);
        }
    }
}

namespace GalaxyCheck.Runners
{
    public class PropertyFailedException : Exception
    {
        private readonly Counterexample<object?> _counterexample;

        public PropertyFailedException(
            Counterexample<object?> counterexample,
            int iterations,
            int shrinks,
            Func<string, string>? formatReproduction,
            Func<string, string>? formatMessage)
            : base(FormatMessage(formatMessage, BuildMessage(counterexample, iterations, shrinks, formatReproduction)))
        {
            _counterexample = counterexample;
        }

        public override string StackTrace => _counterexample.Exception?.StackTrace ?? base.StackTrace!;

        private static string FormatMessage(Func<string, string>? formatMessage, string message) =>
            formatMessage == null
                ? message
                : formatMessage(message);

        private static string BuildMessage(
            Counterexample<object?> counterexample,
            int iterations,
            int shrinks,
            Func<string, string>? formatReproduction) =>
                string.Join(Environment.NewLine, BuildLines(counterexample, iterations, shrinks, formatReproduction));

        private static IEnumerable<string> BuildLines(
            Counterexample<object?> counterexample,
            int iterations,
            int shrinks,
            Func<string, string>? formatReproduction)
        {
            const string LineBreak = "";

            yield return FalsifiedAfterLine(iterations, shrinks);
            yield return ReproductionLine(counterexample, formatReproduction);
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

        private static string FalsifiedAfterLine(int iterations, int shrinks) =>
            $"Falsified after {IterationsTrivia(iterations)} ({ShrinksTrivia(shrinks)})";

        private static string IterationsTrivia(int iterations) => iterations == 1 ? "1 test" : $"{iterations} tests";

        private static string ShrinksTrivia(int shrinks) => shrinks == 1 ? "1 shrink" : $"{shrinks} shrinks";

        private static string ReproductionLine(
            Counterexample<object?> counterexample,
            Func<string, string>? formatReproduction)
        {
            static string PrefixLine(string reproductionFormatted) => $"Reproduction: {reproductionFormatted}";

            if (formatReproduction == null)
            {
                return PrefixLine(counterexample.Replay);
            }
            else
            {
                return PrefixLine(formatReproduction(counterexample.Replay));
            }
        }

        private static string CounterexampleValueLine(Counterexample<object?> counterexample)
        {
            var lines = ExampleRenderer.Render(counterexample.PresentationalValue).ToList();

            return lines.Count switch
            {
                0 => throw new Exception("Fatal: Expected at least one line to be rendered in counterexample"),
                1 => $"Counterexample: {lines.Single()}",
                _ => string.Join(
                    Environment.NewLine,
                    Enumerable.Concat(
                        new[] { "Counterexample:" },
                        lines.Select(l => $"    {l}")))
            };
        }

        private static string ExceptionLine(Exception ex) => $"---- {ex.Message}";
    }
}
