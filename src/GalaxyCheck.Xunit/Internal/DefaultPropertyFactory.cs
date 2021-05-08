using GalaxyCheck.Gens;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    internal class DefaultPropertyFactory : IPropertyFactory
    {
        public IGen<Test<object>> CreateProperty(MethodInfo methodInfo, object? target, IGenFactory? genFactory)
        {
            return Property.Reflect(methodInfo, target, genFactory);
        }
    }
}
