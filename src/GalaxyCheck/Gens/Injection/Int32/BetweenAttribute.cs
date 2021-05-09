using System;

namespace GalaxyCheck.Gens.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class BetweenAttribute : GenAttribute
    {
        public int X { get; }

        public int Y { get; }


        public BetweenAttribute(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override IGen Get => GalaxyCheck.Gen.Int32().Between(X, Y);
    }
}
