using GalaxyCheck.Runners.Check;

namespace GalaxyCheck_Tests_V3.TestUtility;

public record FalsifiedCheckResult<T>(CheckResult<T> OriginalResult)
{
}
