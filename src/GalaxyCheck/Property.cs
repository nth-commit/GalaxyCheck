using GalaxyCheck.Properties;
using System;

namespace GalaxyCheck
{
    public partial class Property : IGen<Test>
    {
        private readonly IGen<Test> _gen;

        public Property(IGen<Test> gen)
        {
            _gen = gen;
        }

        public IGenAdvanced<Test> Advanced => _gen.Advanced;

        IGenAdvanced IGen.Advanced => Advanced;
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