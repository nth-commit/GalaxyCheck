using GalaxyCheck;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    public static class PropertyExtensions
    {
        public static IGen<EvaluatedPropertyIteration<T>> Evaluate<T>(this IProperty<T> property)
        {
            return property.Select(i => new EvaluatedPropertyIteration<T>(i.Input, i.Func(i.Input)));
        }

        public record EvaluatedPropertyIteration<T>(T Input, bool Result);
    }
}
