using GalaxyCheck;
using GalaxyCheck.Abstractions;
using Xunit;

namespace Tests
{
    public static class PropertyAssert
    {
        public static CheckResult.Falsified<T> Falsifies<T>(IProperty<T> property, int seed, ISize? size = null, int? iterations = null)
        {
            var result = property.Check(new RunConfig(iterations: iterations, seed: seed, size: size));

            Assert.True(result is CheckResult.Falsified<T>);

            return (CheckResult.Falsified<T>)result;
        }

        public static CheckResult.Unfalsified<T> DoesNotFalsify<T>(IProperty<T> property, int seed, ISize? size = null, int? iterations = null)
        {
            var result = property.Check(new RunConfig(iterations: iterations, seed: seed, size: size));

            Assert.True(result is CheckResult.Unfalsified<T>);

            return (CheckResult.Unfalsified<T>)result;
        }
    }
}
