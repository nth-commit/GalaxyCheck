using GalaxyCheck.Properties;

namespace GalaxyCheck
{
    public partial class PropertyAsync : IGen<TestAsync>
    {
        private readonly IGen<TestAsync> _gen;

        public PropertyAsync(IGen<TestAsync> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<TestAsync> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;
    }

    public class PropertyAsync<T> : IGen<TestAsync<T>>
    {
        private readonly IGen<TestAsync<T>> _gen;

        public PropertyAsync(IGen<TestAsync<T>> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<TestAsync<T>> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;

        public static implicit operator PropertyAsync(PropertyAsync<T> property) => new PropertyAsync(
            from test in property
            select TestFactory.CreateAsync(new object[] { test.Input }, test.Output, test.PresentedInput));
    }
}
