using GalaxyCheck.Internal;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Tests.GenSnapshotInitializerTests
{
    internal static class TestProxy
    {
        internal static GenSnapshotInitializationResult Initialize(
            Type testClassType,
            string testMethodName,
            IParametersGenFactory? parametersGenFactory = null,
            Action<GlobalGenSnapshotConfiguration>? configure = null)
        {
            var config = new GlobalGenSnapshotConfiguration
            {
                AssertSnapshotMatches = (_) => Task.CompletedTask
            };
            configure?.Invoke(config);

            return GenSnapshotInitializer.Initialize(
                testClassType,
                GetMethod(testClassType, testMethodName),
                new object[] { },
                parametersGenFactory ?? new Mock<IParametersGenFactory>().Object,
                config);
        }

        private static MethodInfo GetMethod(Type type, string name)
        {
            var methodInfo = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);

            if (methodInfo == null)
            {
                throw new Exception("Unable to locate method");
            }

            return methodInfo;
        }
    }
}
