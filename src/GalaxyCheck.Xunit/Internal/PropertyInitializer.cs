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

            var propertiesAttribute = testClassType.GetCustomAttributes<PropertiesAttribute>(inherit: true).FirstOrDefault();
            var propertyAttribute = testMethodInfo.GetCustomAttributes<PropertyAttribute>(inherit: true).Single();

            var genFactory = TryResolveGenFactory(propertyAttribute, propertiesAttribute);
            var property = propertyFactory.CreateProperty(testMethodInfo, testClassInstance, genFactory);
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

        private static IGenFactory? TryResolveGenFactory(PropertyAttribute propertyAttribute, PropertiesAttribute? propertiesAttribute)
        {
            Type? factoryType = propertyAttribute.Factory ?? propertiesAttribute?.Factory;

            if (factoryType == null)
            {
                return null;
            }

            var factoryInterfaces = factoryType.GetInterfaces();
            if (factoryInterfaces.Any(i => i == typeof(IGenFactory)) == false)
            {
                throw new PropertyConfigurationException($"Factory must implement '{typeof(IGenFactory)}' but '{factoryType}' did not.");
            }

            var factoryConstructor = factoryType
                .GetConstructors()
                .Where(c => c.IsPublic && c.GetParameters().Any() == false)
                .SingleOrDefault();
            if (factoryConstructor == null)
            {
                throw new PropertyConfigurationException($"Factory must have a default constructor, but '{factoryType}' did not.");
            }

            return (IGenFactory)factoryConstructor.Invoke(new object[] { });
        }
    }
}
