using System;

namespace GalaxyCheck.Injection
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class BetweenAttribute : Attribute
    {
        public int X { get; }

        public int Y { get; }

        public BetweenAttribute(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
