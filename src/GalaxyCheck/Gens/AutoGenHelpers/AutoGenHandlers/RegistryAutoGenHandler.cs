using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class RegistryAutoGenHandler : IAutoGenHandler
    {
        private readonly IReadOnlyDictionary<Type, IGen> _registeredGensByType;
        private readonly ContextualErrorFactory _errorFactory;

        public RegistryAutoGenHandler(
            IReadOnlyDictionary<Type, IGen> registeredGensByType,
            ContextualErrorFactory errorFactory)
        {
            _registeredGensByType = registeredGensByType;
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            _registeredGensByType.ContainsKey(type);

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            foreach (var kvp in _registeredGensByType)
            {
                var registeredType = kvp.Key;
                var genTypeArgument = ReflectGenTypeArgument(kvp.Value);

                if (registeredType.IsAssignableFrom(genTypeArgument) == false)
                {
                    return _errorFactory(
                        $"type '{genTypeArgument}' was not assignable to the type it was registered to, '{registeredType}'",
                        context);
                }
            }

            return _registeredGensByType[type];
        }

        private static Type ReflectGenTypeArgument(IGen gen)
        {
            var reflectedGenType = gen
                .GetType()
                .GetInterfaces()
                .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IGen<>))
                .SingleOrDefault();

            return reflectedGenType.GetGenericArguments().Single();
        }
    }
}
