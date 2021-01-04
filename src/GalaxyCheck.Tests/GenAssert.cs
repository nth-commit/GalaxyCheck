using GalaxyCheck.Abstractions;
using GalaxyCheck.Aggregators;
using System.Linq;
using Xunit;

namespace GalaxyCheck.Tests
{
    public static class GenAssert
    {
        public static void Equal<T>(IGen<T> expected, IGen<T> actual, int seed)
        {
            var expectedSample = expected.SampleExampleSpaces(opts => opts.WithSeed(seed));
            var actualSample = actual.SampleExampleSpaces(opts => opts.WithSeed(seed));

            Assert.All(
                Enumerable.Zip(expectedSample, actualSample),
                (x) => Assert.Equal(x.First.TraverseGreedy(), x.Second.TraverseGreedy()));
        }

        public static void ShrinksTo<T>(IGen<T> gen, T expected, int seed)
        {
            var actual = gen.Minimal(opts => opts.WithSeed(seed));

            Assert.Equal(expected, actual);
        }

        public static void Errors<T>(IGen<T> gen, string errorMessage, int seed)
        {
            var ex = Assert.Throws<GenErrorException>(() =>
            {
                gen.Sample(opts => opts.WithSeed(seed));
            });

            Assert.Equal(errorMessage, ex.Message);
        }
    }
}
