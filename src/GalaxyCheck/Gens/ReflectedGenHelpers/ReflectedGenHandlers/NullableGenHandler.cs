using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyCheck.Gens.ReflectedGenHelpers.ReflectedGenHandlers
{
    internal class NullableGenHandler : IReflectedGenHandler
    {
        public bool CanHandleGen(Type type, ReflectedGenHandlerContext context) => IsNullableStruct(type);

        public IGen CreateGen(IReflectedGenHandler innerHandler, Type type, ReflectedGenHandlerContext context)
        {
            if (IsNullableStruct(type))
            {
                var nonNullType = type.GetGenericArguments().Single();

                var innerGen = innerHandler.CreateGen(nonNullType, context);

                var methodInfo = typeof(Gen).GetMethod(
                    nameof(Gen.NullableStruct),
                    BindingFlags.Static | BindingFlags.Public)!;
                var genericMethodInfo = methodInfo.MakeGenericMethod(nonNullType);

                return (IGen)genericMethodInfo.Invoke(null!, new object[] { innerGen })!;
            }

            throw new NotImplementedException();
        }

        private static bool IsNullableStruct(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                return genericTypeDefinition == typeof(Nullable<>);
            }

            return false;
        }
    }
}
