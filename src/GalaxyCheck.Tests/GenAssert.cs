using System;
using System.Linq;
using Xunit;
using GalaxyCheck;
using GC = GalaxyCheck;
using Newtonsoft.Json;

namespace Tests
{
    public static class GenAssert
    {
        public static void Equal<T>(IGen<T> expected, IGen<T> actual, int seed, int? iterations = null)
        {
            var expectedSample = expected.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed, iterations: iterations);
            var actualSample = actual.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed, iterations: iterations);

            Assert.All(
                Enumerable.Zip(expectedSample, actualSample),
                (x) => Assert.Equal(x.First.Sample(), x.Second.Sample()));
        }

        public static void ValuesEqual<T>(IGen<T> expected, IGen<T> actual, int seed, int? iterations = null)
        {
            var expectedSample = expected.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed, iterations: iterations);
            var actualSample = actual.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed, iterations: iterations);

            Assert.All(
                Enumerable.Zip(expectedSample, actualSample),
                (x) => Assert.Equal(
                    x.First.Sample().Select(exampleSpace => exampleSpace.Value),
                    x.Second.Sample().Select(exampleSpace => exampleSpace.Value)));
        }

        public static void ShrinksTo<T>(IGen<T> gen, T expected, int seed, Func<T, bool>? pred = null)
        {
            var actual = gen.Minimal(seed: seed, pred: pred);

            Assert.Equal(expected, actual);
        }

        public static void Errors<T>(IGen<T> gen, string errorMessage, int seed)
        {
            var ex = Assert.Throws<GC.Exceptions.GenErrorException>(() =>
            {
                gen.Sample(seed: seed);
            });

            Assert.Equal(errorMessage, ex.Message);
        }
    }
}
