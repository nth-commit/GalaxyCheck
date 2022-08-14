using GalaxyCheck.Gens;
using System.Collections.Generic;
using System.Reflection;

namespace GalaxyCheck.Internal
{
    internal class DefaultParametersGenFactory : IParametersGenFactory
    {
        public IGen<object[]> CreateParametersGen(MethodInfo methodInfo, IGenFactory? genFactory, IReadOnlyDictionary<int, IGen> customGens) =>
            Gen.Parameters(methodInfo, genFactory, customGens);
    }
}
