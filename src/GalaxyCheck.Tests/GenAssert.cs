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

        public static void ShrinksTo<T>(IGen<T> gen, Action<T> assert, int seed, Func<T, bool>? pred = null)
        {
            var actual = gen.Minimal(seed: seed, pred: pred);

            assert(actual);
        }

        public static void DoesNotThrow<T>(IGen<T> gen, int seed)
        {
            gen.Sample(seed: seed);
        }

        public static TException Throws<T, TException>(IGen<T> gen, int seed)
            where TException : Exception
        {
            var ex = Assert.Throws<TException>(() => gen.Sample(seed: seed));
            return ex;
        }

        public static void Errors<T>(IGen<T> gen, string errorMessage, int seed)
        {
            var ex = Throws<T, GC.Exceptions.GenErrorException>(gen, seed);
            Assert.Equal(errorMessage, ex.Message);
        }
    }
}
