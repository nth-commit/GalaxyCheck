using System;

namespace GalaxyCheck.Runners.Check.AsyncAutomata
{
    internal static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExplorationAsync<AsyncTest<T>> CheckTestAsync<T>() => async (example) =>
        {
            var testOutput = await example.Value.Output.Value;
            return testOutput.Result switch
            {
                TestResult.Succeeded => ExplorationOutcome.Success(),
                TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };
    }
}
