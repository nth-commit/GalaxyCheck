using System;

namespace GalaxyCheck.Injection
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class WithBiasAttribute : Attribute
    {
        public Gen.Bias Bias { get; }

        public WithBiasAttribute(Gen.Bias bias)
        {
            Bias = bias;
        }
    }
}
