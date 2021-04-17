using GalaxyCheck;

namespace Tests
{
    public static class PropertyExtensions
    {
        public static IGen<EvaluatedPropertyIteration<T>> Evaluate<T>(this Property<T> property)
        {
            return property.Select(test => new EvaluatedPropertyIteration<T>(test.Input, test.Output.Value.Result == TestResult.Succeeded));
        }

        public record EvaluatedPropertyIteration<T>(T Input, bool Result);
    }
}
