using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.ExampleSpaces
{
    public static partial class ExampleSpaceFactory
    {
        private record ContextualizedUnfoldFunctions<TAccumulator, TContext, TProjection>(
           ContextualShrinkFunc<TAccumulator, TContext> Shrink,
           MeasureFunc<TAccumulator> Measure,
           IdentifyFunc<TAccumulator> Identify,
           Func<TAccumulator, TProjection> Projection,
           NextContextFunc<TProjection, TContext> NextContext);

        /// <summary>
        /// Creates an example space by recursively applying a shrinking function to the root value.
        /// </summary>
        /// <typeparam name="T">The type of the example's value.</typeparam>
        /// <param name="rootValue"></param>
        /// <param name="shrink"></param>
        /// <param name="measure"></param>
        /// <param name="identify"></param>
        /// <returns></returns>
        public static IExampleSpace<T> Unfold<T>(
            T rootValue,
            ShrinkFunc<T> shrink,
            MeasureFunc<T> measure,
            IdentifyFunc<T> identify)
        {
            return Unfold(
                rootValue,
                ShrinkFunc.WithoutContext(shrink),
                measure,
                identify);
        }

        public static IExampleSpace<T> Unfold<T, TContext>(
            T rootValue,
            ContextualShrinker<T, TContext> contextualShrinker,
            MeasureFunc<T> measure,
            IdentifyFunc<T> identify)
        {
            var functions = new ContextualizedUnfoldFunctions<T, TContext, T>(
                contextualShrinker.ContextualShrink, measure, identify, x => x, contextualShrinker.ContextualTraverse);

            var contextualizedExampleSpace = UnfoldHelpers.UnfoldInternal(
                rootValue,
                contextualShrinker.RootContext,
                functions);

            return contextualizedExampleSpace.Map(x => x.Value);
        }

        private static class UnfoldHelpers
        {
            public record ContextualValue<T, TContext>(T Value, TContext Context);

            public static IExampleSpace<ContextualValue<TProjection, TContext>> UnfoldInternal<TAccumulator, TContext, TProjection>(
               TAccumulator accumulator,
               TContext context,
               ContextualizedUnfoldFunctions<TAccumulator, TContext, TProjection> functions)
            {
                var example = CreateExample(accumulator, context, functions);

                var context0 = functions.NextContext(example.Value.Value, context);

                var (context1, shrinks) = functions.Shrink(accumulator, context0);

                var subspace = ContextualizedUnfoldSubspaceInternal(context1, shrinks, functions);

                return new ExampleSpace<ContextualValue<TProjection, TContext>>(example, subspace);
            }

            private static IExample<ContextualValue<TProjection, TContext>> CreateExample<TAccumulator, TContext, TProjection>(
                TAccumulator accumulator,
                TContext context,
                ContextualizedUnfoldFunctions<TAccumulator, TContext, TProjection> functions)
            {
                var id = functions.Identify(accumulator);
                var value = functions.Projection(accumulator);
                var distance = functions.Measure(accumulator);

                return new Example<ContextualValue<TProjection, TContext>>(
                    id,
                    new ContextualValue<TProjection, TContext>(value, context),
                    distance);
            }

            private static IEnumerable<IExampleSpace<ContextualValue<TProjection, TContext>>> ContextualizedUnfoldSubspaceInternal<TAccumulator, TContext, TProjection>(
                TContext context,
                IEnumerable<TAccumulator> accumulators,
                ContextualizedUnfoldFunctions<TAccumulator, TContext, TProjection> functions)
            {
                IExampleSpace<ContextualValue<TProjection, TContext>> GenerateNextExampleSpace(
                    TAccumulator accumulator,
                    TContext context0)
                {
                    return UnfoldInternal(accumulator, context0, functions);
                }

                return TraverseUnencountered(accumulators, context, functions.NextContext, GenerateNextExampleSpace).Select(exampleSpace =>
                {
                    var currentExample = exampleSpace.Current;
                    var context0 = functions.NextContext(currentExample.Value.Value, currentExample.Value.Context);

                    var exampleWithMoreContext = new Example<ContextualValue<TProjection, TContext>>(
                        currentExample.Id,
                        new ContextualValue<TProjection, TContext>(currentExample.Value.Value, context0),
                        currentExample.Distance);

                    return new ExampleSpace<ContextualValue<TProjection, TContext>>(
                        exampleWithMoreContext,
                        exampleSpace.Subspace);
                });
            }

            private static IEnumerable<IExampleSpace<ContextualValue<T, TContext>>> TraverseUnencountered<TAccumulator, TContext, T>(
                IEnumerable<TAccumulator> accumulators,
                TContext context,
                NextContextFunc<T, TContext> nextContext,
                Func<TAccumulator, TContext, IExampleSpace<ContextualValue<T, TContext>>> nextExampleSpace)
            {
                return accumulators
                    .Scan(
                        new UnfoldSubspaceState<T, TContext>(
                            new Option.None<IExampleSpace<ContextualValue<T, TContext>>>(),
                            context),
                        (acc, curr) =>
                        {
                            var exampleSpace = nextExampleSpace(curr, acc.Context);

                            var context0 = nextContext(exampleSpace.Current.Value.Value, acc.Context);

                            return new UnfoldSubspaceState<T, TContext>(
                                new Option.Some<IExampleSpace<ContextualValue<T, TContext>>>(exampleSpace),
                                context0);
                        }
                    )
                    .Select(x => x.UnencounteredExampleSpace switch
                    {
                        Option.Some<IExampleSpace<ContextualValue<T, TContext>>> some => some.Value,
                        _ => null
                    })
                    .Where(exampleSpace => exampleSpace != null)
                    .Cast<IExampleSpace<ContextualValue<T, TContext>>>();
            }

            private class UnfoldSubspaceState<T, TContext>
            {
                public Option<IExampleSpace<ContextualValue<T, TContext>>> UnencounteredExampleSpace { get; init; }

                public TContext Context { get; init; }

                public UnfoldSubspaceState(
                    Option<IExampleSpace<ContextualValue<T, TContext>>> unencounteredExampleSpace,
                    TContext context)
                {
                    UnencounteredExampleSpace = unencounteredExampleSpace;
                    Context = context;
                }
            }
        }
    }
}
