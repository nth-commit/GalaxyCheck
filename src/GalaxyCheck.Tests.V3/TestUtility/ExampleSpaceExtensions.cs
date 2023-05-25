using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Tests.TestUtility;

public static class ExampleSpaceExtensions
{
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
}
