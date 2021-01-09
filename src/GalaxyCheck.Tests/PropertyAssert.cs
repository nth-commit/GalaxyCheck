using GalaxyCheck;
using GalaxyCheck.Abstractions;
using Xunit;

namespace Tests
{
    public static class PropertyAssert
    {
        public static (CheckResult<T>, CheckResultState.Falsified<T>) Falsifies<T>(IProperty<T> property, int seed, ISize? size = null, int? iterations = null)
        {
            var result = property.Check(new RunConfig(iterations: iterations, seed: seed, size: size));

            Assert.True(result.State is CheckResultState.Falsified<T>);

            return (result, (CheckResultState.Falsified<T>)result.State);
        }

        public static (CheckResult<T>, CheckResultState.Unfalsified<T>) DoesNotFalsify<T>(IProperty<T> property, int seed, ISize? size = null, int? iterations = null)
        {
            var result = property.Check(new RunConfig(iterations: iterations, seed: seed, size: size));

            Assert.True(result.State is CheckResultState.Unfalsified<T>);

            return (result, (CheckResultState.Unfalsified<T>)result.State);
        }
    }
}
