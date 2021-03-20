using GalaxyCheck;
using GalaxyCheck.Runners.Check;
using Xunit;

namespace Tests
{
    public static class PropertyAssert
    {
        public static (CheckResult<T>, Counterexample<T>) Falsifies<T>(
            Property<T> property,
            int seed,
            int? size = null,
            int iterations = 100)
        {
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            Assert.True(result.Falsified);

            return (result, result.Counterexample!);
        }

        public static CheckResult<T> DoesNotFalsify<T>(
            Property<T> property,
            int seed,
            int? size = null,
            int iterations = 100)
        {
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            Assert.False(result.Falsified);

            return result;
        }
    }
}
