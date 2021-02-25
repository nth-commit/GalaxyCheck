using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using GalaxyCheck.Internal.Sizing;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.GenIterations;

namespace GalaxyCheck
{
    public static class SelectManyExtensions
    {
        /// <summary>
        /// Projects each value of a generator to a new generator by the given selector. Subspaces of the source
        /// generator and the projected generator are combined through a cross-product.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <typeparam name="TResult">The type of the projected generator's value.</typeparam>
        /// <param name="gen">The generator to apply the projection to.</param>
        /// <param name="selector">A projection function to apply to each value.</param>
        /// <returns>A new generator with the projection applied.</returns>
        public static IGen<TResult> SelectMany<T, TResult>(this IGen<T> gen, Func<T, IGen<TResult>> selector)
        {
            static IEnumerable<IExampleSpace<TResult>> BindSubspace(
                IExampleSpace<T> leftExampleSpace,
                Func<T, IGen<TResult>> selector,
                IRng rng,
                Size size)
            {
                var stream = BindExampleSpace(leftExampleSpace, selector, rng, size);

                foreach (var iteration in stream)
                {
                    if (iteration is GenInstance<TResult> instance)
                    {
                        yield return instance.ExampleSpace;
                    }
                }
            }

            static IExampleSpace<TResult> JoinExampleSpaces(
                IExampleSpace<T> leftExampleSpace,
                IExampleSpace<TResult> rightExampleSpace,
                Func<T, IGen<TResult>> selector,
                IRng rng,
                Size size)
            {
                var jointExampleSpace = rightExampleSpace.MapExamples(example => new Example<TResult>(
                    ExampleId.Combine(leftExampleSpace.Current.Id, example.Id),
                    example.Value,
                    leftExampleSpace.Current.Distance + example.Distance));

                var boundLeftSubspace = leftExampleSpace.Subspace.SelectMany(leftExampleSubspace =>
                    BindSubspace(leftExampleSubspace, selector, rng, size));

                return new ExampleSpace<TResult>(
                    jointExampleSpace.Current,
                    Enumerable.Concat(boundLeftSubspace, rightExampleSpace.Subspace));
            }

            static IEnumerable<IGenIteration<TResult>> BindExampleSpace(
                IExampleSpace<T> exampleSpace,
                Func<T, IGen<TResult>> selector,
                IRng rng,
                Size size)
            {
                return selector(exampleSpace.Current.Value).Advanced
                    .Run(new GenParameters(rng, size))
                    .TakeWhileInclusive(iteration => iteration is not GenInstance<TResult>)
                    .Select(iteration =>
                    {
                        if (iteration is not GenInstance<TResult> instance)
                        {
                            return iteration;
                        }

                        var jointExampleSpace = JoinExampleSpaces(
                            exampleSpace,
                            instance.ExampleSpace,
                            selector,
                            rng,
                            size);

                        return GenIterationBuilder.FromIteration(instance).ToInstance(jointExampleSpace);
                    });
            };

            static IEnumerable<IGenIteration<TResult>> Run(
                IGen<T> gen,
                Func<T, IGen<TResult>> selector,
                IRng rng,
                Size size)
            {
                var stream = gen.Advanced.Run(new GenParameters(rng, size));

                foreach (var iteration in stream)
                {
                    var iterationBuilder = GenIterationBuilder.FromIteration(iteration);

                    if (iteration is not GenInstance<T> instance)
                    {
                        yield return iteration.Match<T, GenIteration<TResult>>(
                            onInstance: _ => throw new NotSupportedException(),
                            onDiscard: discard => iterationBuilder.ToDiscard<TResult>(),
                            onError: error => iterationBuilder.ToError<TResult>(error.GenName, error.Message));
                    }
                    else
                    {
                        var innerStream = BindExampleSpace(
                            instance.ExampleSpace,
                            selector,
                            instance.NextRng,
                            instance.InitialSize);

                        foreach (var innerIteration in innerStream)
                        {
                            var innerIterationBuilder = GenIterationBuilder
                                .FromIteration(iteration)
                                .WithNextRng(innerIteration.NextRng)
                                .WithNextSize(innerIteration.NextSize);

                            yield return innerIteration.Match<TResult, GenIteration<TResult>>(
                                onInstance: instance => innerIterationBuilder.ToInstance(instance.ExampleSpace),
                                onDiscard: discard => innerIterationBuilder.ToDiscard<TResult>(),
                                onError: error => innerIterationBuilder.ToError<TResult>(error.GenName, error.Message));
                        }

                        break;
                    }
                }
            }

            return new FunctionalGen<TResult>((parameters) => Run(gen, selector, parameters.Rng, parameters.Size)).Repeat();
        }

        /// <summary>
        /// Projects each value of a generator to a new generator by the given selector. Subspaces of the source
        /// generator and the projected generator are combined through a cross-product.
        /// </summary>
        /// <typeparam name="T">The type of the generator's value.</typeparam>
        /// <typeparam name="TResult">The type of the projected generator's value.</typeparam>
        /// <param name="gen">The generator to apply the projection to.</param>
        /// <param name="genSelector">A projection function to apply to each value.</param>
        /// <param name="valueSelector">A second projection function to apply.</param>
        /// <returns>A new generator with the projection applied.</returns>
        public static IGen<TResult> SelectMany<T, TResultGen, TResult>(
            this IGen<T> gen,
            Func<T, IGen<TResultGen>> genSelector,
            Func<T, TResultGen, TResult> valueSelector) =>
                gen.SelectMany(x => genSelector(x).Select(y => valueSelector(x, y)));
    }

    public static partial class Gen
    {
        public static IGen<TResult> SelectMany<T0, TResult>(
            IGen<T0> gen0,
            Func<T0, IGen<TResult>> selector) =>
                from x0 in gen0
                from x in selector(x0)
                select x;

        public static IGen<TResult> SelectMany<T0, T1, TResult>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            Func<T0, T1, IGen<TResult>> selector) =>
                from x0 in gen0
                from x1 in gen1
                from x in selector(x0, x1)
                select x;

        public static IGen<TResult> SelectMany<T0, T1, T2, TResult>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            Func<T0, T1, T2, IGen<TResult>> selector) =>
                from x0 in gen0
                from x1 in gen1
                from x2 in gen2
                from x in selector(x0, x1, x2)
                select x;

        public static IGen<TResult> SelectMany<T0, T1, T2, T3, TResult>(
            IGen<T0> gen0,
            IGen<T1> gen1,
            IGen<T2> gen2,
            IGen<T3> gen3,
            Func<T0, T1, T2, T3, IGen<TResult>> selector) =>
                from x0 in gen0
                from x1 in gen1
                from x2 in gen2
                from x3 in gen3
                from x in selector(x0, x1, x2, x3)
                select x;
    }
}
