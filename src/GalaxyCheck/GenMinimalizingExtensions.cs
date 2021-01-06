using GalaxyCheck.Abstractions;
using System.Linq;

namespace GalaxyCheck
{
    public static class GenMinimalizingExtensions
    {
        public static T Minimal<T>(this IGen<T> gen, RunConfig? config = null)
        {
            var sample = gen.Advanced.SampleExampleSpaces(config?.WithIterations(1));

            return sample.First().Minimal()!.Value;
        }
    }
}
