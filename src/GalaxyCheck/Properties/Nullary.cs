using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Property Nullary(Func<bool> func) =>
            new Property<object?>(GalaxyCheck.Gen.Constant<object?>(null), _ => func(), 0);

        public static Property Nullary(Action func) => Nullary(func.AsTrueFunc());
    }
}
