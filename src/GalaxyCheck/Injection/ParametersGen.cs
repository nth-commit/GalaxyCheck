using GalaxyCheck.Internal.GenIterations;
using GalaxyCheck.Internal.Gens;
using GalaxyCheck.Internal.Sizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Injection
{
    public class ParametersGen : BaseGen<object[]>, IGen<object[]>
    {
        private readonly Lazy<IGen<object[]>> _lazyGen;

        public ParametersGen(MethodInfo methodInfo)
        {
            _lazyGen = new Lazy<IGen<object[]>>(() => CreateGen(methodInfo));
        }

        protected override IEnumerable<GenIteration<object[]>> Run(IRng rng, Size size) =>
            _lazyGen.Value.Advanced.Run(rng, size);

        private static IGen<object[]> CreateGen(MethodInfo methodInfo)
        {
            var gens = methodInfo
                .GetParameters()
                .Select(parameterInfo => ResolveGen(parameterInfo));

            return Gen.Zip(gens).Select(xs => xs.ToArray());
        }

        private static IGen<object> ResolveGen(ParameterInfo parameterInfo)
        {
            var gensByType = new Dictionary<Type, Func<ParameterInfo, IGen<object>>>
            {
                { typeof(int), pi => CreateInt32Gen(pi) }
            };

            return gensByType[parameterInfo.ParameterType](parameterInfo);
        }

        private static IGen<object> CreateInt32Gen(ParameterInfo parameterInfo)
        {
            var gen = Gen.Int32();

            var attributes = parameterInfo.GetCustomAttributes();

            var greaterThanEqualAttribute = attributes.OfType<GreaterThanEqualAttribute>().SingleOrDefault();
            if (greaterThanEqualAttribute != null)
            {
                gen = gen.GreaterThanEqual(greaterThanEqualAttribute.Min);
            }

            var lessThanEqualAttribute = attributes.OfType<LessThanEqualAttribute>().SingleOrDefault();
            if (lessThanEqualAttribute != null)
            {
                gen = gen.LessThanEqual(lessThanEqualAttribute.Max);
            }

            var betweenAttribute = attributes.OfType<BetweenAttribute>().SingleOrDefault();
            if (betweenAttribute != null)
            {
                gen = gen.Between(betweenAttribute.X, betweenAttribute.Y);
            }

            var shrinkTowardsAttribute = attributes.OfType<ShrinkTowardsAttribute>().SingleOrDefault();
            if (shrinkTowardsAttribute != null)
            {
                gen = gen.ShrinkTowards(shrinkTowardsAttribute.Origin);
            }

            var withBiasAttribute = attributes.OfType<WithBiasAttribute>().SingleOrDefault();
            if (withBiasAttribute != null)
            {
                gen = gen.WithBias(withBiasAttribute.Bias);
            }

            return gen.Select(x => (object)x);
        }
    }
}
