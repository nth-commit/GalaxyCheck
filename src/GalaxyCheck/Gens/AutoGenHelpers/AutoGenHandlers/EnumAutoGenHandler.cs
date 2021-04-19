using System;
using System.Reflection;

namespace GalaxyCheck.Gens.AutoGenHelpers.AutoGenHandlers
{
    internal class EnumAutoGenHandler : IAutoGenHandler
    {
        public bool CanHandleGen(Type type, AutoGenHandlerContext context) => type.IsEnum;

        public IGen CreateGen(IAutoGenHandler innerHandler, Type type, AutoGenHandlerContext context)
        {
            var genMethodInfo = typeof(Gen).GetMethod(nameof(Gen.Enum), BindingFlags.Public | BindingFlags.Static);
            var genericGenMethodInfo = genMethodInfo.MakeGenericMethod(type);
            return (IGen)genericGenMethodInfo.Invoke(null!, new object[] { });
        }
    }
}
