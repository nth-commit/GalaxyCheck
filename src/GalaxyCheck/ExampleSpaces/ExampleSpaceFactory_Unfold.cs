using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

            var contextualizedExampleSpace = ContextualizedUnfoldInternal(
                rootValue,
                contextualShrinker.RootContext,
                functions,
                ImmutableHashSet.Create<ExampleId>());

            return contextualizedExampleSpace.Map(x => x.Value);
        }

        private record ContextualValue<T, TContext>(T Value, TContext Context);

        private static IExampleSpace<ContextualValue<TProjection, TContext>> ContextualizedUnfoldInternal<TAccumulator, TContext, TProjection>(
           TAccumulator accumulator,
           TContext context,
           ContextualizedUnfoldFunctions<TAccumulator, TContext, TProjection> functions,
           ImmutableHashSet<ExampleId> encountered)
        {
            var example = CreateExample(accumulator, context, functions);

            var encountered0 = encountered.Add(example.Id);
            var context0 = functions.NextContext(example.Value.Value, context);

            var (context1, shrinks) = functions.Shrink(accumulator, context0);

            var subspace = ContextualizedUnfoldSubspaceInternal(context1, shrinks, functions, encountered0);

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
            ContextualizedUnfoldFunctions<TAccumulator, TContext, TProjection> functions,
            ImmutableHashSet<ExampleId> encountered)
        {
            IExampleSpace<ContextualValue<TProjection, TContext>> GenerateNextExampleSpace(
                TAccumulator accumulator,
                TContext context0,
                ImmutableHashSet<ExampleId> encountered)
            {
                return ContextualizedUnfoldInternal(accumulator, context0, functions, encountered);
            }

            return TraverseUnencounteredContextual(accumulators, context, encountered, functions.NextContext, GenerateNextExampleSpace).Select(exampleSpace =>
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
    }
}
