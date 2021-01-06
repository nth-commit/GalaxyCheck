using GalaxyCheck.Abstractions;
using GalaxyCheck.Gens;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public static class Property
    {
        public static IProperty<T0> ForAll<T0>(IGen<T0> gen, Func<T0, bool> func) =>
            new PropertyImpl<T0>(gen.Select<T0, PropertyResult<T0>>(x => func(x)
                ? new PropertyResult.Success<T0>()
                : new PropertyResult.Failure<T0>()));

        private class PropertyImpl<T0> : BaseGen<PropertyResult<T0>>, IProperty<T0>
        {
            private readonly IGen<PropertyResult<T0>> _gen;

            public PropertyImpl(IGen<PropertyResult<T0>> gen)
            {
                _gen = gen;
            }

            protected override IEnumerable<GenIteration<PropertyResult<T0>>> Run(IRng rng, ISize size) =>
                _gen.Advanced.Run(rng, size);
        }
    }
}
