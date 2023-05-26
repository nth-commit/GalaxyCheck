using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeRefactoringProviders
{
    public static class Verifier<TCodeRefactoringProvider>
        where TCodeRefactoringProvider : CodeRefactoringProvider, new()
    {
        public static async Task Verify(
            string code,
            LinePosition codeProvidingRefactorPosition,
            string expectedRefactoredCode,
            string expectedRefactoringTitle)
        {
            var test = new CSharpCodeRefactoringTest<TCodeRefactoringProvider, XUnitVerifier>()
            {
                TestCode = code,
                FixedCode = expectedRefactoredCode,
                CodeActionValidationMode = CodeActionValidationMode.None,
                CodeActionVerifier = (codeAction, verifier) => { verifier.Equal(codeAction.Title, expectedRefactoringTitle); },
                ReferenceAssemblies = new ReferenceAssemblies(
                    "net7.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "7.0.0"),
                    Path.Combine("ref", "net7.0"))
            };

            test.ExpectedDiagnostics.Add(new DiagnosticResult("Refactoring", DiagnosticSeverity.Hidden)
                .WithSpan(codeProvidingRefactorPosition.Line, codeProvidingRefactorPosition.Character, codeProvidingRefactorPosition.Line,
                    codeProvidingRefactorPosition.Character));

            test.OffersEmptyRefactoring = true;

            test.TestState.AdditionalReferences.AddRange(new[]
            {
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(FactAttribute))!.Location),
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IGen))!.Location),
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(PropertyAttribute))!.Location)
            });

            await test.RunAsync();
        }

        public static async Task VerifyNotRefactored(
            string code,
            LinePosition codeProvidingRefactorPosition)
        {
            var test = new CSharpCodeRefactoringTest<TCodeRefactoringProvider, XUnitVerifier>()
            {
                TestCode = code,
                FixedCode = code,
                CodeActionVerifier = (codeAction, verifier) => { verifier.Fail("Expected code not to be refactored"); },
                ReferenceAssemblies = new ReferenceAssemblies(
                    "net7.0",
                    new PackageIdentity(
                        "Microsoft.NETCore.App.Ref",
                        "7.0.0"),
                    Path.Combine("ref", "net7.0"))
            };

            test.ExpectedDiagnostics.Add(new DiagnosticResult("Refactoring", DiagnosticSeverity.Hidden)
                .WithSpan(codeProvidingRefactorPosition.Line, codeProvidingRefactorPosition.Character, codeProvidingRefactorPosition.Line,
                    codeProvidingRefactorPosition.Character));

            test.TestState.AdditionalReferences.AddRange(new[]
            {
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(FactAttribute))!.Location),
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(IGen))!.Location),
                MetadataReference.CreateFromFile(Assembly.GetAssembly(typeof(PropertyAttribute))!.Location)
            });

            await test.RunAsync();
        }
    }
}
