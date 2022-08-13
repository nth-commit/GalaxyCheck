using GalaxyCheck.Configuration;

namespace IntegrationSample
{
    internal class ConfigureGalaxyCheck : IConfigureGlobal
    {
        public void Configure(IGlobalConfiguration instance)
        {
            instance.DefaultIterations = 100;
        }
    }
}
