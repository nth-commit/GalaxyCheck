using Microsoft.CodeAnalysis;
using System.Linq;

namespace GalaxyCheck.Xunit.Analyzers
{
    internal static class SymbolExtensions
    {
        public static bool IsAssignableFrom(
            this ITypeSymbol? targetType,
            ITypeSymbol? sourceType,
            bool exactMatch = false)
        {
            if (targetType is not null)
            {
                while (sourceType is not null)
                {
                    if (SymbolEqualityComparer.Default.Equals(sourceType, targetType))
                        return true;

                    if (exactMatch)
                        return false;

                    if (targetType.TypeKind == TypeKind.Interface)
                        return sourceType.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, targetType));

                    sourceType = sourceType.BaseType;
                }
            }

            return false;
        }
    }
}
