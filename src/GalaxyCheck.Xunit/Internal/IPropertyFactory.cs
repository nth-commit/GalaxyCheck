using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    public interface IPropertyFactory
    {
        Property CreateProperty(
            MethodInfo methodInfo,
            object? target,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen> customGens);
    }
}
