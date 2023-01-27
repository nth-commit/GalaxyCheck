using NebulaCheck.Configuration;

namespace GalaxyCheck_Tests_V3;

public class NebulaCheckConfiguration : IConfigureGlobal
{
    public void Configure(IGlobalConfiguration instance)
    {
        instance.Properties.DefaultIterations = 10;
    }
}
