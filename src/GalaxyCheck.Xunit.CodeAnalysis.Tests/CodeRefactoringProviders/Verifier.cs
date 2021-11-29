using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Testing.Verifiers;
using Microsoft.CodeAnalysis.Text;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace GalaxyCheck.Xunit.CodeAnalysis.Tests.CodeRefactoringProviders
{
    public static class Verifier<TCodeRefactoringProvider>
        where TCodeRefactoringProvider : CodeRefactoringProvider, new()
    {
        public static Task Verify(
            string code,
            string codeProvidingRefactor,
            string expectedRefactoredCode,
            string expectedRefactoringTitle)
        {
            var linePosition = new LinePosition(1, 1);

            var indexOfCodeRefactoringProvider = code.IndexOf(codeProvidingRefactor);


            return Verify(code, linePosition, expectedRefactoredCode, expectedRefactoringTitle);
        }

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
                CodeActionVerifier = (codeAction, verifier) =>
                {
                    verifier.Equal(codeAction.Title, expectedRefactoringTitle);
                }
            };

            test.ExpectedDiagnostics.Add(new DiagnosticResult("Refactoring", DiagnosticSeverity.Hidden)
                .WithSpan(codeProvidingRefactorPosition.Line, codeProvidingRefactorPosition.Character, codeProvidingRefactorPosition.Line, codeProvidingRefactorPosition.Character));

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
                CodeActionVerifier = (codeAction, verifier) =>
                {
                    verifier.Fail("Expected code not to be refactored");
                }
            };

            test.ExpectedDiagnostics.Add(new DiagnosticResult("Refactoring", DiagnosticSeverity.Hidden)
                .WithSpan(codeProvidingRefactorPosition.Line, codeProvidingRefactorPosition.Character, codeProvidingRefactorPosition.Line, codeProvidingRefactorPosition.Character));

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
