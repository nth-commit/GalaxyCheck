using System;
using System.Linq;
using System.Reflection;

namespace GalaxyCheck.Xunit.Internal
{
    public record PropertyInitializationResult(
        IPropertyRunner Runner,
        PropertyRunParameters Parameters,
        string? SkipReason)
    {
        public bool ShouldSkip => SkipReason != null;
    }

    public static class PropertyInitializer
    {
        public static PropertyInitializationResult Initialize(Type testClassType, MethodInfo testMethodInfo, object[] constructorArguments)
        {
            var testClassInstance = Activator.CreateInstance(testClassType, constructorArguments);
            var property = Property.Reflect(testMethodInfo, testClassInstance);

            var propertyAttribute = testMethodInfo.GetCustomAttributes<PropertyAttribute>(inherit: true).Single();
            var replay = testMethodInfo.GetCustomAttributes<ReplayAttribute>().SingleOrDefault()?.Replay;

            var propertyRunParameters = new PropertyRunParameters(
                property,
                propertyAttribute.Iterations,
                propertyAttribute.ShrinkLimit,
                replay);

            return new PropertyInitializationResult(
                propertyAttribute.Runner,
                propertyRunParameters,
                propertyAttribute.Skip?.Length > 0 ? propertyAttribute.Skip : null);
        }
    }
}
