using System;

namespace GalaxyCheck.Runners.Check.AutomataAsync
{
    internal static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExplorationAsync<TestAsync<T>> CheckTestAsync<T>() => async (example) =>
        {
            var testOutput = await example.Value.Output.Value;
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
