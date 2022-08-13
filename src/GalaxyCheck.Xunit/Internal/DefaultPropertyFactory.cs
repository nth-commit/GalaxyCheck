using GalaxyCheck.Gens;
using GalaxyCheck.Internal;
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
            IReadOnlyDictionary<int, IGen> customGens) => Property.ReflectAsync(methodInfo, target, genFactory, customGens);
    }
}
