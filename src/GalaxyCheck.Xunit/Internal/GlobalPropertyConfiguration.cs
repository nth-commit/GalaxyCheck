using GalaxyCheck.Configuration;

namespace GalaxyCheck.Internal
{
    internal class GlobalPropertyConfiguration : IGlobalPropertyConfiguration
    {
        public int DefaultIterations { get; set; } = 100;

        public int DefaultShrinkLimit { get; set; } = 100;
    }
}
