using GalaxyCheck;
using GalaxyCheck.Internal.ExampleSpaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public static List<Example<T>> Sample<T>(
            this ExampleSpace<T> exampleSpace,
            int maxExamples = 10)
        {
            static IEnumerable<Example<T>> SampleRec(ExampleSpace<T> exampleSpace)
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
