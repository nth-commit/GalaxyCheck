using System;

namespace GalaxyCheck.Injection
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class LessThanEqualAttribute : Attribute
    {
        public int Max { get; }

        public LessThanEqualAttribute(int max)
        {
            Max = max;
        }
    }
}
