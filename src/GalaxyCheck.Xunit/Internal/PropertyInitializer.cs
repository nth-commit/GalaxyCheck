using GalaxyCheck.Gens;
using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    internal record PropertyInitializationResult(
        IPropertyRunner Runner,
        PropertyRunParameters Parameters,
        string? SkipReason)
    {
        public bool ShouldSkip => SkipReason != null;
    }

    internal static class PropertyInitializer
    {
        public static PropertyInitializationResult Initialize(
            Type testClassType,
            MethodInfo testMethodInfo,
            object[] constructorArguments,
            IPropertyFactory propertyFactory)
        {
            var testClassInstance = Activator.CreateInstance(testClassType, constructorArguments);
            var propertyAttribute = testMethodInfo.GetCustomAttributes<PropertyAttribute>(inherit: true).Single();

            var genFactory = TryResolveGenFactory(testClassType, testMethodInfo);
            var property = propertyFactory.CreateProperty(testMethodInfo, testClassInstance, genFactory);
            var replay = testMethodInfo.GetCustomAttributes<ReplayAttribute>().SingleOrDefault()?.Replay;

            var propertyRunParameters = new PropertyRunParameters(
                Property: property,
                Iterations: propertyAttribute.Iterations,
                ShrinkLimit: propertyAttribute.ShrinkLimit,
                Replay: replay,
                Seed: replay == null ? propertyAttribute.NullableSeed : null,
                Size: replay == null ? propertyAttribute.NullableSize : null);

            return new PropertyInitializationResult(
                propertyAttribute.Runner,
                propertyRunParameters,
                propertyAttribute.Skip?.Length > 0 ? propertyAttribute.Skip : null);
        }

        private static IGenFactory? TryResolveGenFactory(Type testClassType, MethodInfo testMethodInfo)
        {
            var genFactoryAttribute =
                testMethodInfo.GetCustomAttributes<GenFactoryAttribute>(inherit: true).FirstOrDefault() ??
                testClassType.GetCustomAttributes<GenFactoryAttribute>(inherit: true).FirstOrDefault();

            return genFactoryAttribute?.Value;
        }
    }
}
