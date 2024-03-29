﻿using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static IGen<T> NoShrink<T>(this IGen<T> gen) =>
            gen.TransformInstances(instance => GenIterationFactory.Instance(
                instance.ReplayParameters,
                instance.NextParameters,
                ExampleSpaceFactory.Singleton(instance.ExampleSpace.Current.Id, instance.ExampleSpace.Current.Value)));
    }
}
