using GalaxyCheck.Gens;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    public class DefaultPropertyFactory : IPropertyFactory
    {
        public IGen<Test<object>> CreateProperty(MethodInfo methodInfo, object? target, IAutoGenFactory? autoGenFactory)
        {
            return Property.Reflect(methodInfo, target, autoGenFactory);
        }
    }
}
