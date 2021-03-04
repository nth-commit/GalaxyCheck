using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;

namespace GalaxyCheck
{
    public static class NoShrinkExtensions
    {
        public static IGen<T> NoShrink<T>(this IGen<T> gen) =>
            gen.TransformInstances(instance => GenIterationFactory.Instance(
                instance.RepeatParameters,
                instance.NextParameters,
                ExampleSpaceFactory.Singleton(instance.ExampleSpace.Current.Id, instance.ExampleSpace.Current.Value),
                instance.ExampleSpaceHistory));
    }
}
