using System;

namespace GalaxyCheck.Gens.ReflectedGenHelpers
{
    internal interface IReflectedGenHandler
    {
        bool CanHandleGen(Type type, ReflectedGenHandlerContext context);

        IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context);
    }
}
