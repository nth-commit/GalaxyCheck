using System;

namespace GalaxyCheck.Injection
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ShrinkTowardsAttribute : Attribute
    {
        public int Origin { get; }

        public ShrinkTowardsAttribute(int origin)
        {
            Origin = origin;
        }
    }
}
