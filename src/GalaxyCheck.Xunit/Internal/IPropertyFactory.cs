using GalaxyCheck.Gens;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    public interface IPropertyFactory
    {
        IGen<Test<object>> CreateProperty(MethodInfo methodInfo, object? target, IGenFactory? genFactory);
    }
}
