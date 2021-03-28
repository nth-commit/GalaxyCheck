using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class CompositeAutoGenFactory : IAutoGenFactory
    {
        private readonly IReadOnlyList<IAutoGenFactory> _genFactoriesByPriority;
        private readonly Func<string, IGen> _errorFactory;

        public CompositeAutoGenFactory(
            IReadOnlyList<IAutoGenFactory> genFactoriesByPriority,
            Func<string, IGen> errorFactory)
        {
            _genFactoriesByPriority = genFactoriesByPriority;
            _errorFactory = errorFactory;
        }

        public bool CanHandleType(Type type, AutoGenFactoryContext context) =>
            _genFactoriesByPriority.Any(genFactory => genFactory.CanHandleType(type, context));

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context)
        {
            if (context.TypeHistory.Skip(1).Any(t => t == type))
            {
                return _errorFactory($"detected circular reference on type '{type}'{RenderPathDiagnostics(context)}");
            }

            var gen = _genFactoriesByPriority
                .Where(genFactory => genFactory.CanHandleType(type, context))
                .Select(genFactory => genFactory.CreateGen(innerFactory, type, context))
                .FirstOrDefault();

            if (gen == null)
            {
                return _errorFactory($"could not resolve type '{type}'{RenderPathDiagnostics(context)}");
            }

            return gen;
        }

        private static string RenderPathDiagnostics(AutoGenFactoryContext context) =>
            context.Members.Count() == 1 ? "" : $" at path '{context.MemberPath}'";
    }
}
