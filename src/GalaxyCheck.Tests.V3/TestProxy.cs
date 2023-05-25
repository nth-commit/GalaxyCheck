using System.Reflection;
using GalaxyCheck;
using GalaxyCheck.Runners.Check;

namespace GalaxyCheck_Tests_V3;

public static class TestProxy
{
    public static CheckResult<T> Check<T>(this Property<T> property, Func<CheckPropertyArgs, CheckPropertyArgs>? configure = null)
    {
        var args = CheckPropertyArgs.Default;
        if (configure != null)
        {
            args = configure(args);
        }

        // Rider can't handle static partial extensions :( Completely shits itself.
        var result = typeof(Extensions)
            .GetMethod(nameof(Check))!
            .MakeGenericMethod(typeof(T))
            .Invoke(null, new object?[] { property, null, args.Seed, null, null, null, true })!;
        return (CheckResult<T>)result;
    }

    public record CheckPropertyArgs(int Seed)
    {
        public static CheckPropertyArgs Default { get; } = new(0);
    }
}
