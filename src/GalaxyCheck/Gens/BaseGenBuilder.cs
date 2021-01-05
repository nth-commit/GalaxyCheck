using GalaxyCheck.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens
{
    public class BaseGenBuilder<TInit, TCurr> : IGenBuilder<TCurr>
    {
        private readonly IGen<TInit> _innerGen;
        private readonly Func<IGen<TInit>, IGen<TCurr>> _transformGen;

        public BaseGenBuilder(
            IGen<TInit> innerGen,
            Func<IGen<TInit>, IGen<TCurr>> transformGen)
        {
            _innerGen = innerGen;
            _transformGen = transformGen;
        }

        public IGenBuilder<TNext> Select<TNext>(Func<TCurr, TNext> selector) => Transform<TNext>(gen => (rng, size) =>
        {
            return gen.Run(rng, size).Select(iteration => iteration.Match<TCurr, GenIteration<TNext>>(
                onInstance: (instance) => new GenInstance<TNext>(instance.InitialRng, instance.NextRng, instance.ExampleSpace.Select(selector)),
                onError: (error) => new GenError<TNext>(error.InitialRng, error.NextRng, error.GenName, error.Message)));
        });

        public IEnumerable<GenIteration<TCurr>> Run(IRng rng, ISize size)
        {
            var gen = _transformGen(_innerGen);
            return gen.Run(rng, size);
        }

        private BaseGenBuilder<TInit, TNext> Transform<TNext>(Func<IGen<TCurr>, GenFunc<TNext>> transformGen)
        {
            Func<IGen<TInit>, IGen<TNext>> nextTransformGen = (gen) =>
            {
                var nextGen = _transformGen(gen);
                return new FunctionGen<TNext>(transformGen(nextGen));
            };
            return new BaseGenBuilder<TInit, TNext>(_innerGen, nextTransformGen);
        }
    }

    public class BaseGenBuilder<T> : BaseGenBuilder<T, T>
    {
        public BaseGenBuilder(IGen<T> innerGen)
            : base(innerGen, x => x)
        {
        }
    }
}
