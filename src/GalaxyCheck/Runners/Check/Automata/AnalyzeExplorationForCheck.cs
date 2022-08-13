using System;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExploration<Property.Test<T>> CheckTest<T>() => (example) =>
        {
            var testOutput = example.Value.Output.Value;
            return testOutput.Result switch
            {
                Property.TestResult.Succeeded => ExplorationOutcome.Success(),
                Property.TestResult.Failed => ExplorationOutcome.Fail(testOutput.Exception),
                _ => throw new Exception("Fatal: Unhandled case")
            };
        };
    }
}
