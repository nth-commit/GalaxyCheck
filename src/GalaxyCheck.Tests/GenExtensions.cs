using GalaxyCheck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.V2
{
    public static class GenExtensions
    {
        public static List<List<T>> SampleNTraversals<T>(this IGen<T> gen, int count, int seed, int size) => gen
            .Advanced
            .SampleExampleSpaces(iterations: count, seed: seed, size: size)
            .Select(exs =>
                exs.Traverse()
                    .Take(100)
                    .ToList()).ToList();

        public static List<T> SampleOneTraversal<T>(this IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();

        public static string RenderOneTraversal<T>(this IGen<T> gen, int seed, int size,
            Func<T, string>? renderer = null) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Take(500)
            .Render(x => renderer == null ? x!.ToString()! : renderer(x));
    }
}
