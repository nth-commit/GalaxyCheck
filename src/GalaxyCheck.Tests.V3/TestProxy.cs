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
        try
        {
            var result = typeof(Extensions)
                .GetMethod(nameof(Check))!
                .MakeGenericMethod(typeof(T))
                .Invoke(null, new object?[] { property, args.Size, args.Seed, null, null, args.Replay, true })!;
            return (CheckResult<T>)result;
        }
        catch (TargetInvocationException ex)
        {
            throw ex.InnerException!;
        }
    }

    public static CheckResult<object> Check(this PropertyProxy propertyProxy, Func<CheckPropertyArgs, CheckPropertyArgs>? configure = null)
    {
        var property = Property.ForAll(DummyGens.Object(), propertyProxy.TestFunction);

        return property.Check(args =>
        {
            args = args with
            {
                Seed = propertyProxy.Seed,
                Size = propertyProxy.Size
            };

            if (configure is not null)
            {
                args = configure(args);
            }

            return args;
        });
    }

    /// <summary>
    /// Samples a generator by traversing the example space. This is useful to ensure that not only the produced value satisfies some invariant, but
    /// also all shrunk values.
    /// </summary>
    public static IReadOnlyCollection<T> Sample<T>(this IGen<T> gen, Func<SamplePropertyArgs, SamplePropertyArgs>? configure = null)
    {
        var args = SamplePropertyArgs.Default;
        if (configure != null)
        {
            args = configure(args);
        }

        return gen.Advanced
            .SampleOneExampleSpace(seed: args.Seed, size: args.Size)
            .Traverse()
            .Take(args.MaxSampleSize)
            .ToList();
    }

    public record CheckPropertyArgs(int Seed, int? Size, string? Replay)
    {
        public static CheckPropertyArgs Default { get; } = new(0, null, null);
    }

    public record SamplePropertyArgs(int Seed, int? Size, int MaxSampleSize)
    {
        public static SamplePropertyArgs Default { get; } = new(0, null, 100);
    }
}

public record PropertyProxy(int Seed, int Size, Func<object, bool> TestFunction);
