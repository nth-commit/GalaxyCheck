using System;
using System.Collections.Generic;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenFactories
{
    internal class RegistryAutoGenHandler : IAutoGenHandler
    {
        private readonly IReadOnlyDictionary<Type, IGen> _registeredGensByType;

        public RegistryAutoGenHandler(IReadOnlyDictionary<Type, IGen> registeredGensByType)
        {
            _registeredGensByType = registeredGensByType;
        }

        public bool CanHandleGen(Type type, AutoGenHandlerContext context) =>
            _registeredGensByType.ContainsKey(type);

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context) =>
            _registeredGensByType[type];
    }
}
