using GalaxyCheck.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GalaxyCheck.Internal
{
    internal record GenSnapshotInitializationResult(
        object TestClassInstance,
        string TestFileName,
        Func<ISnapshot, Task> AssertSnapshotMatches,
        IGen<object[]> ParametersGen,
        IReadOnlyList<int> Seeds,
        int Size,
        string? SkipReason)
    {
        public bool ShouldSkip => SkipReason != null;
    }

    internal static class GenSnapshotInitializer
    {
        private static readonly IReadOnlySet<Type> InvalidReturnTypes = new HashSet<Type>(new[] { typeof(void), typeof(Task), typeof(ValueTask) });

        public static GenSnapshotInitializationResult Initialize(
            Type testClassType,
            MethodInfo testMethodInfo,
            object[] constructorArguments,
            IParametersGenFactory parametersGenFactory,
            IGlobalGenSnapshotConfiguration config)
        {
            var assertSnapshotMatches = config.AssertSnapshotMatches;
            if (assertSnapshotMatches == null)
            {
                // TODO: Help article in exception message
                throw new Exception($"Configuration value \"{nameof(IGlobalGenSnapshotConfiguration.AssertSnapshotMatches)}\" needs to be set in order to use gen snapshots");
            }

            if (InvalidReturnTypes.Contains(testMethodInfo.ReturnType))
            {
                throw new Exception("Test method must return a value for snapshotting, or a task containing a value");
            }

            var testClassInstance = Activator.CreateInstance(testClassType, constructorArguments)!;
            var genSnapshotAttribute = testMethodInfo.GetCustomAttributes<GenSnapshotAttribute>(inherit: true).Single();

            var testFileName = genSnapshotAttribute.TestFileName;
            if (testFileName == null)
            {
                // TODO: GitHub issues link
                throw new Exception("Test is incompatible for some reason. Please raise an issue in GitHub.");
            }

            var genFactory = ReflectionHelpers.TryResolveGenFactory(testClassType, testMethodInfo);
            var customGens = ReflectionHelpers.ResolveCustomGens(testClassInstance, testMethodInfo);
            var parametersGen = parametersGenFactory.CreateParametersGen(testMethodInfo, genFactory, customGens);

            var iterations = genSnapshotAttribute.NullableIterations ?? config.DefaultIterations;
            var seeds = Enumerable.Range(0, iterations).ToList();

            var size = genSnapshotAttribute.NullableSize ?? config.DefaultSize;

            return new GenSnapshotInitializationResult(
                testClassInstance,
                testFileName,
                assertSnapshotMatches,
                parametersGen,
                seeds,
                size,
                genSnapshotAttribute.Skip?.Length > 0 ? genSnapshotAttribute.Skip : null);
        }
    }
}
