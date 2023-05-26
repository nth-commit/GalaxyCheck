using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Internal
{
    internal class DefaultPropertyFactory : IPropertyFactory
    {
        public AsyncProperty CreateProperty(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen> customGens,
            object?[]? controlData) => Property.ReflectAsync(methodInfo, target, genFactory, customGens, controlData);
    }
}
