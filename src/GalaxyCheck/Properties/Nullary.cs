using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Property<object[]> Nullary(Func<bool> func) =>
            new Property<object[]>(Gen.Constant(new object[] { }), _ => func(), 0);

        public static Property<object[]> Nullary(Action func) => Nullary(func.AsTrueFunc());
    }
}
