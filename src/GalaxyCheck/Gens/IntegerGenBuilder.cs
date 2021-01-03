using GalaxyCheck.Abstractions;
using GalaxyCheck.ExampleSpaces;

namespace GalaxyCheck.Gens
{
    public interface IIntegerGenBuilder : IGenBuilder<int>
    {
    }

    public class IntegerGenBuilder : BaseGenBuilder<int>, IIntegerGenBuilder
    {
        public static IntegerGenBuilder Create() => new IntegerGenBuilder(new IntegerGenConfig());

        private record IntegerGenConfig
        {
            public IGen<int> ToGen() => new DelayedGen<int>(() => PrimitiveGenBuilder.Create(
                (useNextInt) => useNextInt(int.MinValue, int.MaxValue),
                ShrinkFunc.Towards(0),
                MeasureFunc.Unmeasured<int>()));
        }

        private IntegerGenBuilder(IntegerGenConfig config) : base(config.ToGen())
        {
        }
    }
}
