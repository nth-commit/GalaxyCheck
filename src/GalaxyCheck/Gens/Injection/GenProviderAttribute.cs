using System;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class GenProviderAttribute : Attribute
    {
        public abstract IGen Get { get; }
    }
}
