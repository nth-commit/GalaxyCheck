using GalaxyCheck;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.V2
{
    public static class GenExtensions
    {
        public static List<T> SampleOneTraversal<T>(this IGen<T> gen, int seed, int size) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Traverse()
            .Take(100)
            .ToList();

        public static string RenderOneTraversal<T>(this IGen<T> gen, int seed, int size, Func<T, string>? renderer = null) => gen.Advanced
            .SampleOneExampleSpace(seed: seed, size: size)
            .Take(500)
            .Render(x => renderer == null ? x!.ToString()! : renderer(x));
    }
}
