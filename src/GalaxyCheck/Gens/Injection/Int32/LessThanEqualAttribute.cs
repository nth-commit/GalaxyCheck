using System;

namespace GalaxyCheck.Gens.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class LessThanEqualAttribute : GenAttribute
    {
        public int Max { get; }


        public LessThanEqualAttribute(int max)
        {
            Max = max;
        }

        public override IGen Value => GalaxyCheck.Gen.Int32().LessThanEqual(Max);
    }
}
