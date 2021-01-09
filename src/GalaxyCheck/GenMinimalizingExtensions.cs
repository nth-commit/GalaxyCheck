using GalaxyCheck.Abstractions;
using System;
using System.Linq;

namespace GalaxyCheck
{
    public class NoMinimalFoundException : Exception
    {
    }

    public static class GenMinimalizingExtensions
    {
        public static T Minimal<T>(this IGen<T> gen, RunConfig? config = null, Func<T, bool>? pred = null)
        {
            pred ??= (_) => true;

            var property = gen.ToProperty(x =>
            {
                var result = pred(x);
                return result == false;
            });
            var result = property.Check(config);

            return result.State switch
            {
                CheckResultState.Falsified<T> falsified => falsified.Value,
                _ => throw new NoMinimalFoundException()
            };
        }
    }
}
