using Microsoft.CodeAnalysis;

namespace GalaxyCheck.Xunit.CodeAnalysis
{
    public enum Category
    {
        General
    }

    public static class Descriptors
    {
        public readonly static DiagnosticDescriptor GCX1000_MemberGenMustReferenceExistingMember = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            "GalaxyCheckXunit1000",
#pragma warning restore RS2008 // Enable analyzer release tracking
            "MemberGen must reference an existing member",
            "MemberGen must reference an existing member '{0}' on type '{1}'. Fix the member reference, or add the missing member.",
            "General",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor GCX1001_MemberGenMustReferenceValidMemberType = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            "GalaxyCheckXunit1001",
#pragma warning restore RS2008 // Enable analyzer release tracking
            "MemberGen must reference a valid member type",
            "MemberGen must reference a property or field. Convert the data member to a compatible member type.",
            "General",
            DiagnosticSeverity.Error,
            true);

        public static readonly DiagnosticDescriptor GCX1002_MemberGenMustReferenceMemberOfValidType = new DiagnosticDescriptor(
#pragma warning disable RS2008 // Enable analyzer release tracking
            "GalaxyCheckXunit1002",
#pragma warning restore RS2008 // Enable analyzer release tracking
            "MemberGen must reference a member providing a suitable data type",
            "MemberGen must reference a data type assignable to '{0}'. The referenced type '{1}' is not valid.",
            "General",
            DiagnosticSeverity.Error,
            true);

    }
}
