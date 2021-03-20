using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Internal.Iterations;
using GalaxyCheck.Gens.Iterations.Generic;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Internal.ExampleSpaces;
using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck
{
    public static partial class Extensions
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
                GenParameters parameters)
            {
                var stream = BindExampleSpace(leftExampleSpace, selector, parameters);

                foreach (var iteration in stream)
                {
                    if (iteration.ToEither<TResult, TResult>().IsLeft(out IGenInstance<TResult> instance))
                    {
                        yield return instance.ExampleSpace;
                    }
                }
            }

            static IExampleSpace<TResult> JoinExampleSpaces(
                IExampleSpace<T> leftExampleSpace,
                IExampleSpace<TResult> rightExampleSpace,
                Func<T, IGen<TResult>> selector,
                GenParameters parameters)
            {
                var jointExampleSpace = rightExampleSpace.MapExamples(example => new Example<TResult>(
                    ExampleId.Combine(leftExampleSpace.Current.Id, example.Id),
                    example.Value,
                    leftExampleSpace.Current.Distance + example.Distance));

                var boundLeftSubspace = leftExampleSpace.Subspace.SelectMany(leftExampleSubspace =>
                    BindSubspace(leftExampleSubspace, selector, parameters));

                return ExampleSpaceFactory.Create(
                    jointExampleSpace.Current,
                    Enumerable.Concat(boundLeftSubspace, rightExampleSpace.Subspace));
            }

            static IEnumerable<IGenIteration<TResult>> BindExampleSpace(
                IExampleSpace<T> exampleSpace,
                Func<T, IGen<TResult>> selector,
                GenParameters parameters)
            {
                return selector(exampleSpace.Current.Value).Advanced
                    .Run(parameters)
                    .TakeWhileInclusive(iteration => !iteration.IsInstance())
                    .Select(iteration =>
                    {
                        return iteration.Match(
                            onInstance: instance =>
                            {
                                var jointExampleSpace = JoinExampleSpaces(
                                    exampleSpace,
                                    instance.ExampleSpace,
                                    selector,
                                    parameters);

                                return GenIterationFactory.Instance(
                                    iteration.ReplayParameters,
                                    iteration.NextParameters,
                                    jointExampleSpace);
                            },
                            onError: error => error,
                            onDiscard: discard => discard);
                    });
            };

            static IEnumerable<IGenIteration<TResult>> Run(
                IGen<T> gen,
                Func<T, IGen<TResult>> selector,
                GenParameters parameters)
            {
                var stream = gen.Advanced.Run(parameters);

                foreach (var iteration in stream)
                {
                    var either = iteration.ToEither<T, TResult>();

                    if (either.IsLeft(out IGenInstance<T> instance))
                    {
                        var innerStream = BindExampleSpace(
                            instance.ExampleSpace,
                            selector,
                            instance.ReplayParameters.With(rng: instance.NextParameters.Rng));

                        foreach (var innerIteration in innerStream)
                        {
                            yield return innerIteration.Match(
                                onInstance: innerInstance => GenIterationFactory.Instance(
                                    iteration.ReplayParameters,
                                    innerIteration.NextParameters,
                                    innerInstance.ExampleSpace,
                                    instance.ExampleSpaceHistory),
                                onError: innerError => GenIterationFactory.Error<TResult>(
                                    iteration.ReplayParameters,
                                    innerIteration.NextParameters,
                                    innerError.GenName,
                                    innerError.Message),
                                onDiscard: innerDiscard => GenIterationFactory.Discard<TResult>(
                                    iteration.ReplayParameters,
                                    innerIteration.NextParameters));
                        }

                        break;
                    }
                    else if (either.IsRight(out IGenIteration<TResult> nonInstance))
                    {
                        yield return nonInstance;
                    }
                    else
                    {
                        throw new Exception("Fatal: Unhandled branch");
                    }
                }
            }

            return new FunctionalGen<TResult>((parameters) => Run(gen, selector, parameters)).Repeat();
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
            Func<T, TResultGen, TResult> valueSelector) => gen
                .SelectMany(x => genSelector(x).Select(y =>
                {
                    // Intentionally wrap the left and right values with a symbol, so that we are able to observe the
                    // two values when we need to check the history. You could directly invoke the valueSelector here,
                    // but that would mean that x only appears in scope, and is not written to history.
                    return new { left = x, right = y };
                }))
                .Select(x => valueSelector(x.left, x.right));
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
