using GalaxyCheck;
using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Runners.Check.Automata
{
    internal static class PresentationInferrer
    {
        public static IReadOnlyList<object?> InferValue(IEnumerable<Lazy<IExampleSpace<object>?>> exampleSpaceHistory)
        {
            return InferExampleSpace(exampleSpaceHistory)?.Current.Value ?? new object?[] { };
        }

        public static IExampleSpace<object?[]>? InferExampleSpace(IEnumerable<Lazy<IExampleSpace<object>?>> exampleSpaceHistory)
        {
            var (head, tail) = exampleSpaceHistory.Reverse();

            if (head?.Value == null)
            {
                return null;
            }

            var lastId = head.Value.Current.Id;

            var presentationalExampleSpaces =
                from lazyExs in tail
                let exs = lazyExs.Value
                where exs != null
                where exs.Current.Id == lastId
                where exs.Current.Value == null || IsTest(exs.Current.Value) == false
                select exs;

            return presentationalExampleSpaces.FirstOrDefault()?.Map(UnwrapBinding);
        }

        private static bool IsTest(object obj)
        {
            return obj.GetType().GetInterfaces().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Test<>));
        }

        private static object?[] UnwrapBinding(object? obj)
        {
            static object?[] UnwrapBindingRec(object? obj)
            {
                var type = obj?.GetType();

                if (type != null && type.IsAnonymousType())
                {
                    return type.GetProperties().SelectMany(p => UnwrapBindingRec(p.GetValue(obj))).ToArray();
                }

                return new[] { obj };
            }

            if (obj?.GetType().IsAnonymousType() == true)
            {
                return UnwrapBindingRec(obj);
            }

            return new object?[] { obj };
        }
    }
}
