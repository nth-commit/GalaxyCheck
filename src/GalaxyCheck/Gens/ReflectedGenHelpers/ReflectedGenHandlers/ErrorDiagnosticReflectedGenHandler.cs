using System;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class ErrorDiagnosticReflectedGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) => true;

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            return innerHandler.CreateGen(type, context);
        }
    }
}
