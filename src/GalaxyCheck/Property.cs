using GalaxyCheck.Properties;

namespace GalaxyCheck
{
    public partial class Property : Property<object>
    {
        public Property(IGen<Test<object>> gen) : base(gen)
        {
        }

        public Property(IGen<Test> gen) : this(gen.Select(t => t.Cast<object>()))
        {
        }
    }

    public class Property<T> : IGen<Test<T>>
    {
        private readonly IGen<Test<T>> _gen;

        public Property(IGen<Test<T>> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<Test<T>> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;

        public static implicit operator Property(Property<T> property) => new Property(
            from test in property
            select TestFactory.Create(new object[] { test.Input }, test.Output, test.PresentedInput));
    }
}
