using Microsoft.CodeAnalysis;

namespace GalaxyCheck.Xunit.CodeAnalysis
{
    internal static class CompilationExtensions
    {
        public static INamedTypeSymbol? TryGetPropertyAttributeType(this Compilation compilation) =>
            compilation.GetTypeByMetadataName("GalaxyCheck.PropertyAttribute");

        public static INamedTypeSymbol? TryGetGenSnapshotAttributeType(this Compilation compilation) =>
            compilation.GetTypeByMetadataName("GalaxyCheck.GenSnapshotAttribute");

        public static INamedTypeSymbol? TryGetMemberGenAttributeType(this Compilation compilation) =>
            compilation.GetTypeByMetadataName("GalaxyCheck.MemberGenAttribute");
    }
}
