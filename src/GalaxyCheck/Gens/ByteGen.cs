namespace GalaxyCheck
{
    using GalaxyCheck.Gens;

    public static partial class Gen
    {
        public static IByteGen Byte() => new ByteGen();
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;

    public interface IIntegerGen<T> : IGen<T>
    {
        IIntegerGen<T> GreaterThanEqual(T min);

        IIntegerGen<T> LessThanEqual(T max);

        IIntegerGen<T> ShrinkTowards(T origin);

        IIntegerGen<T> WithBias(Gen.Bias bias);
    }

    public interface IByteGen : IIntegerGen<byte>
    {
    }

    internal class ByteGen : BaseGen<byte>, IByteGen
    {
        private readonly ByteGenConfig _config;

        private record ByteGenConfig(
            byte? Min,
            byte? Max,
            byte? Origin,
            Gen.Bias? Bias);

        private ByteGen(ByteGenConfig config)
        {
            _config = config;
        }

        public ByteGen() : this(new ByteGenConfig(Min: null, Max: null, Origin: null, Bias: null))
        {
        }

        public IIntegerGen<byte> GreaterThanEqual(byte min) => new ByteGen(_config with { Min = min });

        public IIntegerGen<byte> LessThanEqual(byte max) => new ByteGen(_config with { Max = max });

        public IIntegerGen<byte> ShrinkTowards(byte origin) => new ByteGen(_config with { Origin = origin });

        public IIntegerGen<byte> WithBias(Gen.Bias bias) => new ByteGen(_config with { Bias = bias });

        protected override IEnumerable<IGenIteration<byte>> Run(GenParameters parameters) =>
            CreateGen(_config).Advanced.Run(parameters);

        private static IGen<byte> CreateGen(ByteGenConfig config)
        {
            var gen = Gen
                .Int32()
                .GreaterThanEqual(config.Min ?? 0)
                .LessThanEqual(config.Max ?? byte.MaxValue);

            if (config.Origin != null)
            {
                gen = gen.ShrinkTowards(config.Origin.Value);
            }

            if (config.Bias != null)
            {
                gen = gen.WithBias(config.Bias.Value);
            }

            return gen.Select(x => (byte)x);
        }
    }
}
