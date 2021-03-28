using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        public bool CanHandleType(Type type) =>
            _genFactoriesByPriority.Any(genFactory => genFactory.CanHandleType(type));

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, ImmutableStack<(string name, Type type)> path)
        {
            if (path.Skip(1).Any(item => item.type == type))
            {
                return _errorFactory($"detected circular reference on type '{type}'{RenderPathDiagnostics(path)}");
            }

            var gen = _genFactoriesByPriority
                .Where(genFactory => genFactory.CanHandleType(type))
                .Select(genFactory => genFactory.CreateGen(innerFactory, type, path))
                .FirstOrDefault();

            if (gen == null)
            {
                return _errorFactory($"could not resolve type '{type}'{RenderPathDiagnostics(path)}");
            }

            return gen;
        }

        private static string RenderPathDiagnostics(ImmutableStack<(string name, Type type)> path) =>
            path.Count() == 1 ? "" : $" at path '{RenderPath(path)}'";

        private static string RenderPath(ImmutableStack<(string name, Type type)> path) =>
            string.Join(".", path.Reverse().Select(item => item.name));
    }
}
