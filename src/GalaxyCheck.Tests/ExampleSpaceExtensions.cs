using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public static class ExampleSpaceExtensions
    {
        public static List<LocatedExample<T>> TraverseGreedy<T>(
            this IExampleSpace<T> exampleSpace,
            int maxExamples = 10) =>
                exampleSpace.Traverse().Take(maxExamples).ToList();

        public static string Render<T>(
            this IExampleSpace<T> exampleSpace,
            Func<T, string> renderValue,
            int indentation = 2,
            int maxExamples = 500)
        {
            string RenderExample(LocatedExample<T> example)
            {
                var renderedValue = renderValue(example.Value);
                var padding = Enumerable.Range(0, indentation * example.LevelIndex).Aggregate("", (acc, _) => acc + ".");
                return $"{padding}{renderedValue}";
            }

            var lines = exampleSpace.Traverse().Take(maxExamples).Select(RenderExample);

            return string.Join(Environment.NewLine, lines);
        }
    }
}
