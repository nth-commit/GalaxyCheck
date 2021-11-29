using Microsoft.CodeAnalysis;

namespace GalaxyCheck.Xunit.CodeAnalysis
{
    internal static class CompilationExtensions
    {
        public static INamedTypeSymbol? TryGetPropertyAttributeType(this Compilation compilation) =>
            compilation.GetTypeByMetadataName("GalaxyCheck.PropertyAttribute");

        public static INamedTypeSymbol? TryGetMemberGenAttributeType(this Compilation compilation) =>
            compilation.GetTypeByMetadataName("GalaxyCheck.MemberGenAttribute");
    }
}
