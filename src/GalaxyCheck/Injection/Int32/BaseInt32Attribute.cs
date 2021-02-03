using GalaxyCheck.Gens;
using System;

namespace GalaxyCheck.Injection.Int32
{
    public abstract class BaseInt32Attribute : Attribute, IGenInjectionConfigurationFilter<int, IInt32Gen>
    {
        public abstract IInt32Gen Configure(IInt32Gen gen);
    }
}
