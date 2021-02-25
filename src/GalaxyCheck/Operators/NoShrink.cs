using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using System.Linq;

namespace GalaxyCheck
{
    public static class NoShrinkExtensions
    {
        public static IGen<T> NoShrink<T>(this IGen<T> gen) =>
            gen.TransformInstances(instance => new GenInstance<T>(
                instance.InitialRng,
                instance.InitialSize,
                instance.NextRng,
                instance.NextSize,
                ExampleSpace.Singleton(instance.ExampleSpace.Current.Id, instance.ExampleSpace.Current.Value)));
    }
}
