using GalaxyCheck;
using GalaxyCheck.Runners.Check;

namespace GalaxyCheck_Tests_V3.TestUtility;

public static class PropertyExtensions
{
    public static Counterexample<T> EnsureFalsified<T>(this Property<T> property)
    {
        return property.Check().Counterexample!;
    }
}
