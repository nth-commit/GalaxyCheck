using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class RegistryAutoGenFactory : IAutoGenFactory
    {
        private readonly IReadOnlyDictionary<Type, IGen> _registeredGensByType;

        public RegistryAutoGenFactory(IReadOnlyDictionary<Type, IGen> registeredGensByType)
        {
            _registeredGensByType = registeredGensByType;
        }

        public bool CanHandleType(Type type, AutoGenFactoryContext context) =>
            _registeredGensByType.ContainsKey(type);

        public IGen CreateGen(IAutoGenFactory innerFactory, Type type, AutoGenFactoryContext context) =>
            _registeredGensByType[type];
    }
}
