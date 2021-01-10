using GalaxyCheck;
using Xunit;

namespace Tests
{
    public static class PropertyAssert
    {
        public static (CheckResult<T>, CheckResultState.Falsified<T>) Falsifies<T>(
            IProperty<T> property,
            int seed,
            int? size = null,
            int? iterations = null)
        {
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            Assert.True(result.State is CheckResultState.Falsified<T>);

            return (result, (CheckResultState.Falsified<T>)result.State);
        }

        public static (CheckResult<T>, CheckResultState.Unfalsified<T>) DoesNotFalsify<T>(
            IProperty<T> property,
            int seed,
            int? size = null,
            int? iterations = null)
        {
            var result = property.Check(iterations: iterations, seed: seed, size: size);

            Assert.True(result.State is CheckResultState.Unfalsified<T>);

            return (result, (CheckResultState.Unfalsified<T>)result.State);
        }
    }
}
