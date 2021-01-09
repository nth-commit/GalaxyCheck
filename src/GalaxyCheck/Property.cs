using GalaxyCheck.Abstractions;
using GalaxyCheck.Gens;
using System;
using System.Collections.Generic;

namespace GalaxyCheck
{
    public static class Property
    {
        public static IProperty<T> ForAll<T>(IGen<T> gen, Func<T, bool> func) =>
            new PropertyImpl<T>(gen.Select(x => new PropertyIteration<T>(func.Invoke, x)));

        private class PropertyImpl<T0> : BaseGen<PropertyIteration<T0>>, IProperty<T0>
        {
            private readonly IGen<PropertyIteration<T0>> _gen;

            public PropertyImpl(IGen<PropertyIteration<T0>> gen)
            {
                _gen = gen;
            }

            protected override IEnumerable<GenIteration<PropertyIteration<T0>>> Run(IRng rng, ISize size) =>
                _gen.Advanced.Run(rng, size);
        }
    }
}
