using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalaxyCheck.Tests.ExampleSpaces
{
    public static class ExampleSpaceExtensions
    {
        public static List<Example<TResult>> TraverseGreedy<TSource, TResult>(
            this ExampleSpace<TSource, TResult> exampleSpace,
            int maxExamples = 10) =>
                exampleSpace.Traverse().Take(maxExamples).ToList();
    }
}
