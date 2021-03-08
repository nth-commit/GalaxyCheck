using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Test ForThese(Func<bool> func) => new TestImpl(
            (_) => func(),
            new Symbols.NoInput(),
            0);

        public static Test ForThese(Action func) => ForThese(func.AsTrueFunc());
    }
}
