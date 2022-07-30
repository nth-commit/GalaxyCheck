using System;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExploration<Test<T>> CheckTest<T>() => (example) =>
        {
            var testOutput = example.Value.Output.Value;
            return testOutput.Result switch
            {
                TestResult.Succeeded => ExplorationOutcome.Success(),
                TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };
    }
}
