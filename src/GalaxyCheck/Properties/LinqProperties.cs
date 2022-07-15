using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Test ForThese(Func<bool> func) => TestFactory.Create(
            null!,
            () => func(),
            null);

        public static Test ForThese(Action func) => ForThese(func.AsTrueFunc());
    }
}
