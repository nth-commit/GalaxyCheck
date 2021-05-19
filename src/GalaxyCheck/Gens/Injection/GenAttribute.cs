using System;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public abstract class GenAttribute : Attribute
    {
        public abstract IGen Value { get; }
    }
}
