﻿using GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;
using Newtonsoft.Json;
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
    }
}
