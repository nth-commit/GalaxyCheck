using GalaxyCheck.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class RegistryReflectedGenHandler : IReflectedGenHandler
    {
        private readonly IReadOnlyDictionary<Type, Func<IGen>> _registeredGensByType;
        private readonly ContextualErrorFactory _errorFactory;

        public RegistryReflectedGenHandler(
            IReadOnlyDictionary<Type, Func<IGen>> registeredGensByType,
            ContextualErrorFactory errorFactory)
        {
            _registeredGensByType = registeredGensByType;
            _errorFactory = errorFactory;
        }

        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) =>
            _registeredGensByType.ContainsKey(type);

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            var gen = _registeredGensByType[type]();

            var genTypeArgument = gen.ReflectGenTypeArgument();
            if (type.IsAssignableFrom(genTypeArgument) == false)
            {
                return _errorFactory(
                    $"type '{genTypeArgument}' was not assignable to the type it was registered to, '{type}'",
                    context);
            }

            return gen;
        }
    }
}
