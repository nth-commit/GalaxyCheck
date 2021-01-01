using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GalaxyCheck.ExampleSpaces
{
    public delegate IEnumerable<T> ShrinkFunc<T>(T value);

    public static class ShrinkFunc
    {
        public static ShrinkFunc<T> None<T>() => (T _) => Enumerable.Empty<T>();
    }
}
