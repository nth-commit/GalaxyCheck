using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property
    {
        public static Test<object[]> ForThese(Func<bool> func) => new Property<object[]>.TestImpl(
            (_) => func(),
            null!,
            0,
            true);

        public static Test<object[]> ForThese(Action func) => ForThese(func.AsTrueFunc());
    }
}
