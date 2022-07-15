using System;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExploration<Test<T>> Impl<T>() => (example) =>
        {
            var testOutput = example.Value.Output.Value;
            return testOutput.Result switch
            {
                TestResult.Succeeded => ExplorationOutcome.Success(),
                TestResult.FailedPrecondition => ExplorationOutcome.Discard(),
                TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };
    }
}
