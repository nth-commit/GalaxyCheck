using System;
using System.Linq;

namespace GalaxyCheck.Internal
{
    internal static class GenExtensions
    {
        public static Type ReflectGenTypeArgument(this IGen gen)
        {
            var reflectedGenType = gen
                .GetType()
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGen<>))
                .Single();

            return reflectedGenType.GetGenericArguments().Single();
        }
    }
}
