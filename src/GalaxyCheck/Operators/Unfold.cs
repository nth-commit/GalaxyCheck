using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public static partial class Extensions
    {
        public static IGen<T> Unfold<T>(
            this IGenAdvanced<T> advanced,
            Func<T, IEnumerable<T>> shrinkValue,
            Func<T, decimal>? measureValue = null,
            Func<T, int>? identifyValue = null)
        {
            return advanced.Unfold(value => ExampleSpaceFactory.Unfold(
                value,
                shrinkValue.Invoke,
                measureValue == null ? MeasureFunc.Unmeasured<T>() : measureValue.Invoke,
                identifyValue == null ? IdentifyFuncs.Default<T>() : value0 => ExampleId.Primitive(identifyValue(value0))));
        }

        public static IGen<T> Unfold<T>(
            this IGenAdvanced<T> advanced,
            Func<T, IExampleSpace<T>> unfolder)
        {
            GenInstanceTransformation<T, T> transformation = (instance) =>
            {
                return GenIterationFactory.Instance(
                    instance.RepeatParameters,
                    instance.NextParameters,
                    unfolder(instance.ExampleSpace.Current.Value),
                    instance.ExampleSpaceHistory);
            };

            return advanced
                .AsGen()
                .TransformInstances(transformation);
        }
    }
}
