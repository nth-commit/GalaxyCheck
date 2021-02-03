using GalaxyCheck.Gens;
using System;

namespace GalaxyCheck.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class LessThanEqualAttribute : BaseInt32Attribute
    {
        public int Max { get; }

        public LessThanEqualAttribute(int max)
        {
            Max = max;
        }

        public override IInt32Gen Configure(IInt32Gen gen) => gen.LessThanEqual(Max);
    }
}
