using FluentAssertions;
using GalaxyCheck;
using GalaxyCheck.ExampleSpaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.V2
{
    public static class GenAssert
    {
        public static void Equal<T>(IGen<T> expected, IGen<T> actual, int seed, int? iterations = null)
        {
            var expectedSample = expected.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed, iterations: iterations);
            var actualSample = actual.Select(x => JsonConvert.SerializeObject(x)).Advanced.SampleExampleSpaces(seed: seed, iterations: iterations);

            Assert.All(
                Enumerable.Zip(expectedSample, actualSample),
                (x) =>
                {
                    Assert.Equal(x.First.Sample(), x.Second.Sample());
                });
        }

        public static List<IExample<T>> Sample<T>(
            this IExampleSpace<T> exampleSpace,
            int maxExamples = 10)
        {
            static IEnumerable<IExample<T>> SampleRec(IExampleSpace<T> exampleSpace)
            {
                yield return exampleSpace.Current;

                foreach (var subExampleSpace in exampleSpace.Subspace.SelectMany(es => SampleRec(es)))
                {
                    yield return subExampleSpace;
                }
            }

            return SampleRec(exampleSpace).Take(maxExamples).ToList();
        }

        public static void Errors<T>(IGen<T> gen, int? seed = null, string? expectedMessage = null)
        {
            Action test = () => gen.SampleOne(seed: seed ?? 0);

            if (expectedMessage == null)
            {
                test.Should()
                    .Throw<Exceptions.GenErrorException>();
            }
            else
            {
                test.Should()
                    .Throw<Exceptions.GenErrorException>()
                    .WithMessage(expectedMessage);
            }
        }
    }
}
