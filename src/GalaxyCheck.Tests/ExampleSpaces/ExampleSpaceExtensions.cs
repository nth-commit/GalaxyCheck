using GalaxyCheck.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Tests.ExampleSpaces
{
    public static class ExampleSpaceExtensions
    {
        public static List<Example<T>> TraverseGreedy<T>(
            this ExampleSpace<T> exampleSpace,
            int maxExamples = 10) =>
                exampleSpace.Traverse().Take(maxExamples).ToList();

        public static string Render<T>(
            this ExampleSpace<T> exampleSpace,
            Func<T, string> renderValue,
            int indentation = 2)
        {
            string RenderExample(Example<T> example, int level)
            {
                var renderedValue = renderValue(example.Value);
                var padding = Enumerable.Range(0, indentation * level).Aggregate("", (acc, _) => acc + ".");
                return $"{padding}{renderedValue}";
            }

            IEnumerable<string> RenderExampleSpace(PopulatedExampleSpace<T> exampleSpace, int level)
            {
                yield return RenderExample(exampleSpace.Current, level);

                foreach (var line in exampleSpace.Subspace.SelectMany(childExampleSubspace => RenderExampleSpace(childExampleSubspace, level + 1)))
                {
                    yield return line;
                }
            }

            var lines = exampleSpace switch
            {
                EmptyExampleSpace<T> _ => Enumerable.Empty<string>(),
                PopulatedExampleSpace<T> populatedExampleSpace => RenderExampleSpace(populatedExampleSpace, 0),
                _ => throw new NotSupportedException()
            };

            return string.Join(Environment.NewLine, lines);
        }
    }
}
