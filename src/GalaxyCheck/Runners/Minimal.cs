using System;

namespace GalaxyCheck
{
    public static class MinimalExtensions
    {
        public static T Minimal<T>(
            this IGen<T> gen,
            int iterations = 100,
            int? seed = null,
            int? size = null,
            Func<T, bool>? pred = null)
        {
            pred ??= (_) => true;

            var property = gen.ForAll(x =>
            {
                var result = pred(x);
                return result == false;
            });
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            if (result.Falsified)
            {
                return result.Counterexample!.Value;
            }

            throw new Exceptions.NoMinimalFoundException();
        }
    }
}
