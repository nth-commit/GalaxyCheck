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
            Func<T, string> renderValue)
        {
            var lines = RenderLines(exampleSpace, renderValue).ToList();

            return string.Join(Environment.NewLine, lines);
        }

        private static IEnumerable<string> RenderLines<T>(IExampleSpace<T> exampleSpace, Func<T, string> renderValue)
        {
            // TODO: Would be more efficient if an example exposed that it was terminal (the last one of a shrink)
            // TODO: Measure the max space of a value, pad all the renderings by that
            // TODO: Fucking grinds to a halt on big spaces

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

        public static IEnumerable<T> Traverse<T>(this IExampleSpace<T> exampleSpace) =>
            exampleSpace.TraverseExamples().Select(example => example.Value);

        public static IEnumerable<IExample<T>> TraverseExamples<T>(this IExampleSpace<T> exampleSpace)
        {
            yield return exampleSpace.Current;

            foreach (var exampleSubSpace in exampleSpace.Subspace)
            {
                foreach (var subExample in TraverseExamples(exampleSubSpace))
                {
                    yield return subExample;
                }
            }
        }

        public static IExampleSpace<T> Take<T>(this IExampleSpace<T> exampleSpace, int count)
        {
            static (IExampleSpace<T>? exampleSpace, int countTaken) TakeRec(IExampleSpace<T> exampleSpace, int count)
            {
                if (count <= 0)
                {
                    return (null, 0);
                }

                var takenSubspace = new List<IExampleSpace<T>>();
                var countTaken = 1;

                foreach (var subExampleSpace in exampleSpace.Subspace)
                {
                    if (countTaken >= count)
                    {
                        break;
                    }

                    var (takenSubExampleSpace, subCountTaken) = TakeRec(subExampleSpace, count - countTaken);
                    
                    if (takenSubExampleSpace != null)
                    {
                        takenSubspace.Add(takenSubExampleSpace);
                    }

                    countTaken += subCountTaken;
                }

                var takenExampleSpace = ExampleSpaceFactory.Create<T>(exampleSpace.Current, takenSubspace);

                return (takenExampleSpace, countTaken);
            }

            return TakeRec(exampleSpace, count).exampleSpace!;
        }
    }
}
