using GalaxyCheck.Internal;
using System;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Internal.PropertyDiscoverer", "GalaxyCheck.Xunit")]
    public class PropertyAttribute : FactAttribute
    {
        public int Iterations
        {
            get
            {
                if (NullableIterations == null)
                {
                    throw new InvalidOperationException($"{nameof(Iterations)} was not set, and does not have a default value");
                }

                throw new InvalidOperationException("");
            }
            set
            {
                NullableIterations = value;
            }
        }

        internal int? NullableIterations { get; private set; }

        public int ShrinkLimit
        {
            get
            {
                if (NullableShrinkLimit == null)
                {
                    throw new InvalidOperationException($"{nameof(ShrinkLimit)} was not set, and does not have a default value");
                }

                throw new InvalidOperationException("");
            }
            set
            {
                NullableShrinkLimit = value;
            }
        }

        internal int? NullableShrinkLimit { get; private set; }

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
