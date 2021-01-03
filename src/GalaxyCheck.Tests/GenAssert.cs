using GalaxyCheck.Abstractions;
using System.Linq;
using Xunit;

namespace GalaxyCheck.Tests
{
    public static class GenAssert
    {
        public static void Equal<T>(IGen<T> expected, IGen<T> actual, int seed)
        {
            var expectedSample = expected.Sample(opts => opts.WithSeed(seed));
            var actualSample = actual.Sample(opts => opts.WithSeed(seed));

            Assert.All(Enumerable.Zip(expectedSample, actualSample), (x) => Assert.Equal(x.First, x.Second));
        }
    }
}
