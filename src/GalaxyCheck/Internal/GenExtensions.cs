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

        public static Type ReflectGenTypeArgument(this Func<IGen> gen)
        {
            var genType = gen.GetType().GetGenericArguments().Single();

            var reflectedGenType = genType
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGen<>))
                .Single();

            return reflectedGenType.GetGenericArguments().Single();
        }
    }
}
