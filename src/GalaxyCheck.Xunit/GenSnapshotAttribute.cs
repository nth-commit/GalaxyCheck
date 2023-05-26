using System;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Sdk;

namespace GalaxyCheck
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [XunitTestCaseDiscoverer("GalaxyCheck.Internal.Infrastructure.Xunit.GenSnapshotDiscoverer", "GalaxyCheck.Xunit")]
    public sealed class GenSnapshotAttribute : FactAttribute
    {
        public GenSnapshotAttribute([CallerFilePath] string? testFilePath = null)
        {
            TestFileName = testFilePath;
        }

        public string? TestFileName { get; }

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

        internal int? NullableSize { get; private set; }

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

        internal int? NullableIterations { get; private set; }
    }
}
