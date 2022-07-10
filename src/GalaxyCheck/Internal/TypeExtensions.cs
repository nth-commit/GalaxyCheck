using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GalaxyCheck.Internal
{
    internal static class TypeExtensions
    {
        public static bool IsAnonymousType(this Type type)
        {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            bool nameContainsAnonymousType = type?.FullName?.Contains("AnonymousType") == true;
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;

            return isAnonymousType;
        }
    }
}
