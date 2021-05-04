using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class CompositeAutoGenHandler : IAutoGenHandler
    {
        private readonly IReadOnlyList<IAutoGenHandler> _genHandlersByPriority;
        private readonly ContextualErrorFactory _errorFactory;

        public CompositeAutoGenHandler(
            IReadOnlyList<IAutoGenHandler> genHandlersByPriority,
            ContextualErrorFactory errorFactory)
        {
            _genHandlersByPriority = genHandlersByPriority;
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            _genHandlersByPriority.Any(genFactory => genFactory.CanHandleGen(type, context));

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            if (context.TypeHistory.Skip(1).Any(t => t == type))
            {
                return _errorFactory($"detected circular reference on type '{type}'", context);
            }

            var gen = _genHandlersByPriority
                .Where(genFactory => genFactory.CanHandleGen(type, context))
                .Select(genFactory => genFactory.CreateGen(innerHandler, type, context))
                .FirstOrDefault();

            if (gen == null)
            {
                return _errorFactory($"could not resolve type '{type}'", context);
            }

            return gen;
        }
    }
}
