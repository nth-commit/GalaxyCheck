using GalaxyCheck.Configuration;
using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Internal
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
            IPropertyFactory propertyFactory,
            IGlobalPropertyConfiguration config)
        {
            var testClassInstance = Activator.CreateInstance(testClassType, constructorArguments)!;
            var propertyAttribute = testMethodInfo.GetCustomAttributes<PropertyAttribute>(inherit: true).Single();

            var genFactory = ReflectionHelpers.TryResolveGenFactory(testClassType, testMethodInfo);
            var customGens = ReflectionHelpers.ResolveCustomGens(testClassInstance, testMethodInfo);
            var property = propertyFactory.CreateProperty(testMethodInfo, testClassInstance, genFactory, customGens);
            var replay = testMethodInfo.GetCustomAttributes<ReplayAttribute>().SingleOrDefault()?.Replay;

            var propertyRunParameters = new PropertyRunParameters(
                Property: property,
                Iterations: propertyAttribute.NullableIterations ?? config.DefaultIterations,
                ShrinkLimit: propertyAttribute.NullableShrinkLimit ?? config.DefaultShrinkLimit,
                Replay: replay,
                Seed: replay == null ? propertyAttribute.NullableSeed : null,
                Size: replay == null ? propertyAttribute.NullableSize : null);

            return new PropertyInitializationResult(
                propertyAttribute.Runner,
                propertyRunParameters,
                propertyAttribute.Skip?.Length > 0 ? propertyAttribute.Skip : null);
        }
    }
}
