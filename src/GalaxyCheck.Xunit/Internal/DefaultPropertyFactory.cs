using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    internal class DefaultPropertyFactory : IPropertyFactory
    {
        public IGen<Test<object>> CreateProperty(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen> customGens)
        {
            return Property.Reflect(methodInfo, target, genFactory, customGens);
        }
    }
}
