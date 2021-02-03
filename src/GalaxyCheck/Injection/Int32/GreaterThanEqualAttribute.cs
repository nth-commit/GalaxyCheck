using GalaxyCheck.Gens;
using System;

namespace GalaxyCheck.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class GreaterThanEqualAttribute : BaseInt32Attribute
    {
        public int Min { get; }

        public GreaterThanEqualAttribute(int min)
        {
            Min = min;
        }

        public override IInt32Gen Configure(IInt32Gen gen) => gen.GreaterThanEqual(Min);
    }
}
