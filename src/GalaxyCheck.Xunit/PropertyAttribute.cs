using GalaxyCheck.Xunit.Internal;
using System;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Xunit.Internal.PropertyDiscoverer", "GalaxyCheck.Xunit")]
    public class PropertyAttribute : FactAttribute
    {
        public int ShrinkLimit { get; set; } = 500;

        public int Iterations { get; set; } = 100;

        public int Seed
        {
            get
            {
                if (NullableSeed == null)
                {
                    throw new InvalidOperationException($"{nameof(Seed)} was not set, and does not have a default value");
                }

                throw new InvalidOperationException("");
            }
            set
            {
                NullableSeed = value;
            }
        }

        internal int? NullableSeed { get; private set; }

        public int Size
        {
            get
            {
                if (NullableSize == null)
                {
                    throw new InvalidOperationException($"{nameof(Size)} was not set, and does not have a default value");
                }

                throw new InvalidOperationException("");
            }
            set
            {
                NullableSize = value;
            }
        }

        internal int? NullableSize { get; private set; }

        internal virtual IPropertyRunner Runner => new PropertyAssertRunner();
    }
}
