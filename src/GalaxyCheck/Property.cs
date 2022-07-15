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

    public class AsyncProperty : AsyncProperty<object>
    {
        public AsyncProperty(IGen<AsyncTest<object>> gen) : base(gen)
        {
        }

        public AsyncProperty(IGen<AsyncTest> gen) : this(gen.Select(t => t.Cast<object>()))
        {
        }
    }

    public class AsyncProperty<T> : IGen<AsyncTest<T>>
    {
        private readonly IGen<AsyncTest<T>> _gen;

        public AsyncProperty(IGen<AsyncTest<T>> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<AsyncTest<T>> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;

        public static implicit operator AsyncProperty(AsyncProperty<T> property) => new AsyncProperty(
            from test in property
            select TestFactory.Create(new object[] { test.Input }, test.Output, test.PresentedInput));
    }
}
