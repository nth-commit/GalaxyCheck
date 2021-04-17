using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Test<object> ForThese(Func<bool> func) => TestFactory.Create<object>(
            null!,
            new Lazy<bool>(() => func()),
            null);

        public static Test<object> ForThese(Action func) => ForThese(func.AsTrueFunc());
    }
}
