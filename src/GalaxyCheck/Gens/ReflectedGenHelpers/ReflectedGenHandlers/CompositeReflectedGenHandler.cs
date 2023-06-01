using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class CompositeReflectedGenHandler : IReflectedGenHandler
    {
        private readonly IReadOnlyList<IReflectedGenHandler> _genHandlersByPriority;

        public CompositeReflectedGenHandler(IReadOnlyList<IReflectedGenHandler> genHandlersByPriority)
        {
            _genHandlersByPriority = genHandlersByPriority;
        }

        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            _genHandlersByPriority.Any(genFactory => genFactory.CanHandleGen(type, context));

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            if (context.TypeHistory.Skip(1).Any(t => t == type))
            {
                return context.Error(type, $"detected circular reference on type '{type}'");
            }

            var gen = _genHandlersByPriority
                .Where(genFactory => genFactory.CanHandleGen(type, context))
                .Select(genFactory => genFactory.CreateGen(innerHandler, type, context))
                .FirstOrDefault();

            if (gen == null)
            {
                return context.Error(type, $"could not resolve type '{type}'");
            }

            return gen;
        }
    }
}
