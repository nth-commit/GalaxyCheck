using System;
using System.Collections.Generic;
using System.Text;

namespace GalaxyCheck.ExampleSpaces
{
    public delegate int MeasureFunc<T>(T value);

    public static class MeasureFunc
    {
        public static MeasureFunc<T> Unmeasured<T>() => (T _) => 0;
    }
}
