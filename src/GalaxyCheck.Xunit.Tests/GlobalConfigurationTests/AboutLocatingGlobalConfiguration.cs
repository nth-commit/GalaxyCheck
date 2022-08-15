using FluentAssertions;
using GalaxyCheck.Configuration;
using GalaxyCheck.Internal;
using Xunit;

namespace Tests.GlobalConfigurationTests
{
    public class AboutLocatingGlobalConfiguration
    {
        [Fact]
        public void ItWorks()
        {
            var instance = GlobalConfiguration.GetInstance(typeof(MyConfigureGlobal).Assembly);

            instance.Properties.DefaultIterations.Should().Be(420);
        }
    }

    public class MyConfigureGlobal : IConfigureGlobal
    {
        public void Configure(IGlobalConfiguration instance)
        {
            instance.Properties.DefaultIterations = 420;
        }
    }
}
