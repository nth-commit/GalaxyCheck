using GalaxyCheck.Internal;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal static class CheckStateEnumerator
    {
        public static IEnumerable<CheckStateTransition<T>> Enumerate<T>(
            CheckState<T> initialState,
            CheckStateContext<T> initialContext,
            IReadOnlyCollection<ICheckStateTransitionDecorator<T>> decorators) => EnumerableExtensions.Unfold(
                new CheckStateTransition<T>(initialState, initialContext),
                previousTransition =>
                {
                    if (previousTransition.State is TerminationState<T>)
                    {
                        return new Option.None<CheckStateTransition<T>>();
                    }

                    var transition = previousTransition.State.Transition(previousTransition.Context);

                    var decoratedTransition = decorators
                        .Scan(
                            (previousState: previousTransition.State, nextTransition: transition),
                            (acc, decorator) =>
                            {
                                var nextTransition = decorator.Decorate(acc.previousState, acc.nextTransition);
                                return (previousState: acc.nextTransition.State, nextTransition);
                            })
                        .Select(x => x.nextTransition)
                        .Last();

                    return new Option.Some<CheckStateTransition<T>>(decoratedTransition);
                });
    }
}
