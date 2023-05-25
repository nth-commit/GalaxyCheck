using GalaxyCheck.Stable.Configuration;

namespace GalaxyCheck.Tests;

public class GalaxyCheckConfiguration : IConfigureGlobal
{
    public void Configure(IGlobalConfiguration instance)
    {
        instance.Properties.DefaultIterations = 10;
    }
}
