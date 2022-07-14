using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    internal class PropertyFactoryAsync : IPropertyFactory
    {
        public IGen<TestAsync<object>> CreateProperty(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen> customGens) => Property.ReflectAsync(methodInfo, target, genFactory, customGens);
    }
}
