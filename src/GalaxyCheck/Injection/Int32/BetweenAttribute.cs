using GalaxyCheck.Gens;
using System;

namespace GalaxyCheck.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class BetweenAttribute : BaseInt32Attribute
    {
        public int X { get; }

        public int Y { get; }

        public BetweenAttribute(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override IInt32Gen Configure(IInt32Gen gen) => gen.Between(X, Y);
    }
}
