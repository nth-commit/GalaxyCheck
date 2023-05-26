using GalaxyCheck.Configuration;

namespace IntegrationSample
{
    public class GalaxyCheckConfiguration : IConfigureGlobal
    {
        public void Configure(IGlobalConfiguration instance)
        {
            instance.Properties.DefaultIterations = 10;
            instance.Properties.Seed = 42;
        }
    }

}
