using System;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public static class AnalyzeExplorationForCheck
    {
        public static AnalyzeExploration<Test<T>> Impl<T>() => (example) =>
        {
            try
            {
                var success = example.Value.Output.Value;
                return success ? ExplorationOutcome.Success() : ExplorationOutcome.Fail(null);
            }
            catch (Property.PropertyPreconditionException)
            {
                return ExplorationOutcome.Discard();
            }
            catch (Exception ex)
            {
                return ExplorationOutcome.Fail(ex);
            }
        };
    }
}
