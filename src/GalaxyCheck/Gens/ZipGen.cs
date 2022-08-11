using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Gen
    {
        public static IGen<(T0, T1)> Zip<T0, T1>(IGen<T0> gen0, IGen<T1> gen1) =>
            from x0 in gen0
            from x1 in gen1
            select (x0, x1);

        public static IGen<(T0, T1, T2)> Zip<T0, T1, T2>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2) =>
                from x0 in gen0
                from x1 in gen1
                from x2 in gen2
                select (x0, x1, x2);

        public static IGen<(T0, T1, T2, T3)> Zip<T0, T1, T2, T3>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3) =>
                from x0 in gen0
                from x1 in gen1
                from x2 in gen2
                from x3 in gen3
                select (x0, x1, x2, x3);

        public static IGen<IEnumerable<T>> Zip<T>(IEnumerable<IGen<T>> gens)
        {
            var gensList = gens.ToList();
            var shrink = ShrinkFunc.None<IReadOnlyCollection<IExampleSpace<T>>>();

            return new FunctionalGen<IEnumerable<T>>(RunOnce).Repeat();

            IEnumerable<IGenIteration<IEnumerable<T>>> RunOnce(GenParameters parameters)
            {
                var nextParameters = parameters;
                var exampleSpaces = new List<IExampleSpace<T>>(gensList.Count);

                foreach (var gen in gensList)
                {
                    IGenInstance<T>? instance = null;

                    foreach (var iteration in gen.Advanced.Run(nextParameters))
                    {
                        var either = iteration.ToEither<T, IReadOnlyList<T>>();
                        if (either.IsLeft(out IGenInstance<T> instance0))
                        {
                            instance = instance0;
                            break;
                        }
                        else if (either.IsRight(out IGenIteration<IReadOnlyList<T>> nonInstance))
                        {
                            yield return nonInstance;
                        }
                        else
                        {
                            throw new System.Exception("Unhandled case");
                        }
                    }

                    if (instance == null)
                    {
                        throw new System.Exception("Fatal: Element generator exhausted");
                    }
                    else
                    {
                        exampleSpaces.Add(instance.ExampleSpace);
                        nextParameters = instance.NextParameters;
                    }
                }

                var exampleSpace = ExampleSpaceFactory.Merge(
                    exampleSpaces,
                    shrink,
                    exampleSpaces => exampleSpaces.Sum(exs => exs.Current.Distance),
                    enableSmallestExampleSpacesOptimization: false,
                    enableUniqueIds: false);

                yield return GenIterationFactory.Instance(parameters, nextParameters, exampleSpace);
            }
        }
    }
}
