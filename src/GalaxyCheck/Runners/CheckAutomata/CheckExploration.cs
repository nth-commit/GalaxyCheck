using System;

namespace GalaxyCheck.Runners.CheckAutomata
{
    internal static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExploration<Test<T>> Impl<T>() => (example) =>
        {
            var testOutput = example.Value.Output.Value;
            return testOutput.Result switch
            {
                Test.TestResult.Succeeded => ExplorationOutcome.Success(),
                Test.TestResult.FailedPrecondition => ExplorationOutcome.Discard(),
                Test.TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };
    }
}
