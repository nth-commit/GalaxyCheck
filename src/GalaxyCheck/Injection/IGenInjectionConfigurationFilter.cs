namespace GalaxyCheck.Injection
{
    public interface IGenInjectionConfigurationFilter<TValue, TGen>
        where TGen : IGen<TValue>
    {
        TGen Configure(TGen gen);
    }
}
