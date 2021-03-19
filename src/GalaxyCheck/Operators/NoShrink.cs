using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Internal.ExampleSpaces;

namespace GalaxyCheck
{
    public static class NoShrinkExtensions
    {
        public static IGen<T> NoShrink<T>(this IGen<T> gen) =>
            gen.TransformInstances(instance => GenIterationFactory.Instance(
                instance.ReplayParameters,
                instance.NextParameters,
                ExampleSpaceFactory.Singleton(instance.ExampleSpace.Current.Id, instance.ExampleSpace.Current.Value),
                instance.ExampleSpaceHistory));
    }
}
