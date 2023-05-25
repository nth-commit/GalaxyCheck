using GalaxyCheck.Runners.Check;

namespace GalaxyCheck.Tests.TestUtility;

public record FalsifiedCheckResult<T>(CheckResult<T> OriginalResult)
{
}
