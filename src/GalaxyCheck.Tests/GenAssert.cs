using GalaxyCheck;
using GalaxyCheck.Abstractions;
using GalaxyCheck.Aggregators;
using System.Linq;
using Xunit;

namespace Tests
{
    public static class GenAssert
    {
        public static void Equal<T>(IGen<T> expected, IGen<T> actual, int seed)
        {
            var config = new RunConfig(seed: seed);
            var expectedSample = expected.SampleExampleSpaces(config);
            var actualSample = actual.SampleExampleSpaces(config);

            Assert.All(
                Enumerable.Zip(expectedSample, actualSample),
                (x) => Assert.Equal(x.First.TraverseGreedy(), x.Second.TraverseGreedy()));
        }

        public static void ShrinksTo<T>(IGen<T> gen, T expected, int seed)
        {
            var actual = gen.Minimal(new RunConfig(seed: seed));

            Assert.Equal(expected, actual);
        }

        public static void Errors<T>(IGen<T> gen, string errorMessage, int seed)
        {
            var config = new RunConfig(seed: seed);

            var ex = Assert.Throws<GenErrorException>(() =>
            {
                gen.Sample(config);
            });

            Assert.Equal(errorMessage, ex.Message);
        }
    }
}
