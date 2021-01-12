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
        public static void Equal<T>(IGen<T> expected, IGen<T> actual, int seed)
        {
            var expectedSample = expected.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed);
            var actualSample = actual.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed);

            Assert.All(
                Enumerable.Zip(expectedSample, actualSample),
                (x) => Assert.Equal(x.First.Sample(), x.Second.Sample()));
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
