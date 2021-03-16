using GalaxyCheck.Internal.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Internal.ExampleSpaces
{
    public static partial class ExampleSpaceFactory
    {
        private static IEnumerable<IExampleSpace<T>> TraverseUnencountered<TAccumulator, T>(
            IEnumerable<TAccumulator> accumulators,
            ImmutableHashSet<ExampleId> encountered,
            Func<TAccumulator, ImmutableHashSet<ExampleId>, IExampleSpace<T>> nextExampleSpace)
        {
            var contextualExampleSpaces = TraverseUnencounteredContextual(
                accumulators,
                new NoContext(),
                encountered,
                (_, context) => context,
                (accumulator, _, encountered) =>
                    nextExampleSpace(accumulator, encountered).Map(value => new ContextualValue<T, NoContext>(value, new NoContext())));

            return contextualExampleSpaces.Select(exampleSpace => exampleSpace.Map(value => value.Value));
        }

        private static IEnumerable<IExampleSpace<ContextualValue<T, TContext>>> TraverseUnencounteredContextual<TAccumulator, TContext, T>(
            IEnumerable<TAccumulator> accumulators,
            TContext context,
            ImmutableHashSet<ExampleId> encountered,
            NextContextFunc<T, TContext> nextContext,
            Func<TAccumulator, TContext, ImmutableHashSet<ExampleId>, IExampleSpace<ContextualValue<T, TContext>>> nextExampleSpace)
        {
            return accumulators
                .Scan(
                    new UnfoldSubspaceState<T, TContext>(
                        new Option.None<IExampleSpace<ContextualValue<T, TContext>>>(),
                        context,
                        encountered),
                    (acc, curr) =>
                    {
                        var exampleSpace = nextExampleSpace(curr, acc.Context, acc.Encountered);

                        var hasBeenEncountered = acc.Encountered.Contains(exampleSpace.Current.Id);
                        if (hasBeenEncountered)
                        {
                            return new UnfoldSubspaceState<T, TContext>(
                                new Option.None<IExampleSpace<ContextualValue<T, TContext>>>(),
                                acc.Context, // Use the previous context, so the discarded value is not considered
                                acc.Encountered);
                        }

                        var context0 = nextContext(exampleSpace.Current.Value.Value, acc.Context);

                        return new UnfoldSubspaceState<T, TContext>(
                            new Option.Some<IExampleSpace<ContextualValue<T, TContext>>>(exampleSpace),
                            context0,
                            acc.Encountered.Add(exampleSpace.Current.Id));
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

            public ImmutableHashSet<ExampleId> Encountered { get; init; }

            public UnfoldSubspaceState(
                Option<IExampleSpace<ContextualValue<T, TContext>>> unencounteredExampleSpace,
                TContext context,
                ImmutableHashSet<ExampleId> encountered)
            {
                UnencounteredExampleSpace = unencounteredExampleSpace;
                Context = context;
                Encountered = encountered;
            }
        }
    }
}
