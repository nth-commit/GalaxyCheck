using GalaxyCheck.Gens;
using System;

namespace GalaxyCheck.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class ShrinkTowardsAttribute : BaseInt32Attribute
    {
        public int Origin { get; }

        public ShrinkTowardsAttribute(int origin)
        {
            Origin = origin;
        }

        public override IInt32Gen Configure(IInt32Gen gen) => gen.ShrinkTowards(Origin);
    }
}
