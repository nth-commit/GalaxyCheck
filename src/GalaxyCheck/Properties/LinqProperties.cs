using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Test ForThese(Func<bool> func) => new TestImpl(
            (_) => func(),
            null!,
            0,
            true);

        public static Test ForThese(Action func) => ForThese(func.AsTrueFunc());
    }
}
