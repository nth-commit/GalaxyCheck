using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Internal
{
    public interface IParametersGenFactory
    {
        IGen<object[]> CreateParametersGen(
            MethodInfo methodInfo,
            IGenFactory? genFactory,
            IReadOnlyDictionary<int, IGen> customGens);
    }
}
