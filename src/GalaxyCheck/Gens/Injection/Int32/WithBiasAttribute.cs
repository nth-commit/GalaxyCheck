using System;

namespace GalaxyCheck.Gens.Injection.Int32
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class WithBiasAttribute : BaseInt32Attribute
    {
        public Gen.Bias Bias { get; }

        public WithBiasAttribute(Gen.Bias bias)
        {
            Bias = bias;
        }

        public override IInt32Gen Configure(IInt32Gen gen) => gen.WithBias(Bias);
    }
}
