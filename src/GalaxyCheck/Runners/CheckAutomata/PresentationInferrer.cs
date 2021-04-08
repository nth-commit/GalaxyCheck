using GalaxyCheck.ExampleSpaces;
using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Runners.CheckAutomata
{
    public static class PresentationInferrer
    {
        public static object? InferValue(IEnumerable<Lazy<IExampleSpace<object>?>> exampleSpaceHistory)
        {
            return InferExampleSpace(exampleSpaceHistory)?.Current.Value;
        }

        public static IExampleSpace<object?>? InferExampleSpace(IEnumerable<Lazy<IExampleSpace<object>?>> exampleSpaceHistory)
        {
            var (head, tail) = exampleSpaceHistory.Reverse();

            if (head?.Value == null)
            {
                return null;
            }

            var lastHashCode = head.Value.Current.Id.HashCode;

            var presentationalExampleSpaces =
                from lazyExs in tail
                let exs = lazyExs.Value
                where exs != null
                where exs.Current.Id.HashCode == lastHashCode
                where exs.Current.Value == null || IsTest(exs.Current.Value.GetType()) == false
                select exs;

            return presentationalExampleSpaces.FirstOrDefault()?.Map(UnwrapBinding);
        }

        private static bool IsTest(Type type) => (
            from i in type.GetInterfaces()
            where i.IsGenericType
            select i.GetGenericTypeDefinition()
        ).Contains(typeof(Test<>).GetGenericTypeDefinition());

        private static object? UnwrapBinding(object? obj)
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

            return obj;
        }
    }
}
