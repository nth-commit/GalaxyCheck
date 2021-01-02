using GalaxyCheck.ExampleSpaces;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Tests.ExampleSpaces.ExampleSpace
{
    public static class ExampleSpaceExtensions
    {
        public static List<Example<TResult>> TraverseGreedy<TSource, TResult>(
            this ExampleSpace<TSource, TResult> exampleSpace,
            int maxExamples = 10) =>
                exampleSpace.Traverse().Take(maxExamples).ToList();
    }
}
