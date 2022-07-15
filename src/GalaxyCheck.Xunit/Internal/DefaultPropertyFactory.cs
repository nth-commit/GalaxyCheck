using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    internal class DefaultPropertyFactory : IPropertyFactory
    {
        public Property CreateProperty(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen> customGens) => Property.Reflect(methodInfo, target, genFactory, customGens);
    }
}
