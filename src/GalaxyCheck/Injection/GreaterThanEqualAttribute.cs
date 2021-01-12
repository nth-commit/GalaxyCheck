using System;

namespace GalaxyCheck.Injection
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GreaterThanEqualAttribute : Attribute
    {
        public int Min { get; }

        public GreaterThanEqualAttribute(int min)
        {
            Min = min;
        }
    }
}
