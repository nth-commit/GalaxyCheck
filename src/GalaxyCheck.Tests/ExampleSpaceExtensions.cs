using GalaxyCheck.Internal.ExampleSpaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    public static class ExampleSpaceExtensions
    {
        public static List<IExample<T>> Sample<T>(
            this IExampleSpace<T> exampleSpace,
            int maxExamples = 10)
        {
            static IEnumerable<IExample<T>> SampleRec(IExampleSpace<T> exampleSpace)
            {
                yield return exampleSpace.Current;

                foreach (var subExampleSpace in exampleSpace.Subspace.SelectMany(es => SampleRec(es)))
                {
                    yield return subExampleSpace;
                }
            }

            return SampleRec(exampleSpace).Take(maxExamples).ToList();
        }

        public static string Render<T>(
            this IExampleSpace<T> exampleSpace,
            Func<T, string> renderValue,
            int indentation = 2,
            int maxExamples = 500)
        {
            string RenderExample(IExample<T> example, int level)
            {
                var renderedValue = renderValue(example.Value);
                var padding = Enumerable.Range(0, indentation * level).Aggregate("", (acc, _) => acc + ".");
                return $"{padding}{renderedValue}";
            }

            IEnumerable<string> RenderExampleSpace(IExampleSpace<T> exampleSpace, int level)
            {
                yield return RenderExample(exampleSpace.Current, level);

                foreach (var renderedSubSpace in exampleSpace.Subspace.SelectMany(subExampleSpace => RenderExampleSpace(subExampleSpace, level + 1)))
                {
                    yield return renderedSubSpace;
                }
            }

            var lines = RenderExampleSpace(exampleSpace, 0).Take(maxExamples).ToList();

            return string.Join(Environment.NewLine, lines);
        }
    }
}
