using GalaxyCheck.Internal.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.V2
{
    public static class ExampleSpaceExtensions
    {
        public static string Render<T>(
            this IExampleSpace<T> exampleSpace,
            Func<T, string> renderValue,
            int maxExamples = 500)
        {
            var lines = RenderLines(exampleSpace, renderValue).Take(maxExamples).ToList();

            return string.Join(Environment.NewLine, lines);
        }

        private static IEnumerable<string> RenderLines<T>(IExampleSpace<T> exampleSpace, Func<T, string> renderValue)
        {
            // TODO: Would be more efficient if an example exposed that it was terminal (the last one of a shrink)
            // TODO: Measure the max space of a value, pad all the renderings by that

            var example = exampleSpace.Current;

            var distance = example.Distance;
            var distanceRounded = Math.Round(example.Distance, 4);
            var distanceRendered = distance == distanceRounded ? distanceRounded.ToString() : $"{distanceRounded}...";

            yield return $"{renderValue(example.Value)} (Id = {example.Id.HashCode}, Distance = {distanceRendered})";

            var subExampleSpaces = exampleSpace.Subspace.ToList();

            for (var index = 0; index < subExampleSpaces.Count; index++)
            {
                var subExampleSpace = subExampleSpaces[index];
                var isLastSubExampleSpace = index == subExampleSpaces.Count - 1;
                var firstPrefix = isLastSubExampleSpace ? "└> " : "├> ";
                var otherPrefix = isLastSubExampleSpace ? "   " : "|  ";

                var subLines = RenderLines(subExampleSpace, renderValue).ToList();

                for (var subIndex = 0; subIndex < subLines.Count; subIndex++)
                {
                    if (subIndex == 0)
                    {
                        yield return $"{firstPrefix}{subLines[subIndex]}";
                    }
                    else
                    {
                        yield return $"{otherPrefix}{subLines[subIndex]}";
                    }
                }
            }
        }
    }
}
