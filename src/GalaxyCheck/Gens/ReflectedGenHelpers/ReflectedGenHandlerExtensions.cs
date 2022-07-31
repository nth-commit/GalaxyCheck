using GalaxyCheck.Gens.Internal;
using GalaxyCheck.Gens.Iterations;
using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal static class ReflectedGenHandlerExtensions
    {
        public static IGen CreateGen(
            this IReflectedGenHandler innerHandler,
            Type type,
            ReflectedGenHandlerContext context) => innerHandler.CreateGen(innerHandler, type, context);

        public static IGen CreateNamedGen(
            this IReflectedGenHandler innerHandler,
            Type elementType,
            string elementName,
            ReflectedGenHandlerContext parentContext)
        {
            var context = parentContext.Next(elementName, elementType);
            return innerHandler
                .CreateGen(elementType, context)
                .WithTransformedParameters(parameters =>
                {
                    var pathDependendentSeed = Encoding.Unicode
                        .GetBytes(context.MemberPath)
                        .Select(x => (int)x)
                        .Aggregate((acc, curr) =>
                        {
                            unchecked
                            {
                                return acc + curr;
                            }
                        });

                    var nextRng = parameters.Rng.Influence(pathDependendentSeed);
                    var nextParameters = GenParameters.Create(Rng.Create(pathDependendentSeed), parameters.Size);

                    return nextParameters;
                });
        }
    }
}
