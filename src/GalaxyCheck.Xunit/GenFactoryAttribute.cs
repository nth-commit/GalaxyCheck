using GalaxyCheck.Gens;
using System;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public abstract class GenFactoryAttribute : Attribute
    {
        public abstract IGenFactory Get { get; }
    }
}
