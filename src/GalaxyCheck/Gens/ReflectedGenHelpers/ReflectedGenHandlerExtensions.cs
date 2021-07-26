using System;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal static class ReflectedGenHandlerExtensions
    {
        public static IGen CreateGen(
            this IReflectedGenHandler innerHandler,
            Type type,
            ReflectedGenHandlerContext context) =>
                innerHandler.CreateGen(innerHandler, type, context);
    }
}
