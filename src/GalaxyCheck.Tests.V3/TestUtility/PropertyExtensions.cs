using GalaxyCheck.Runners.Check;

namespace GalaxyCheck.Tests.TestUtility;

public static class PropertyExtensions
{
    public static Counterexample<T> EnsureFalsified<T>(this Property<T> property)
    {
        return property.Check().Counterexample!;
    }
}
