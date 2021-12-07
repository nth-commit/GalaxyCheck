using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace GalaxyCheck.Xunit.CodeAnalysis.CodeRefactoringProviders
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp)]
    public class ConfigureGenWithMember : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var document = context.Document;

            var root = await document.GetSyntaxRootAsync(context.CancellationToken);
            if (root is null) return;

            var semanticModel = await document.GetSemanticModelAsync(context.CancellationToken);
            if (semanticModel is null) return;

            var propertyAttributeType = semanticModel.Compilation.TryGetPropertyAttributeType();
            if (propertyAttributeType is null) return;

            var memberGenAttributeType = semanticModel.Compilation.TryGetMemberGenAttributeType();
            if (memberGenAttributeType is null) return;

            var activeNode = root.FindNode(context.Span);

            var activeMethodDeclarationSyntax = activeNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (activeMethodDeclarationSyntax is null) return;

            var propertyAttribute = activeMethodDeclarationSyntax
                .AttributeLists
                .SelectMany(l => l.Attributes)
                .FirstOrDefault(a =>
                {
                    var attributeSymbol = semanticModel.GetSymbolInfo(a);
                    return SymbolEqualityComparer.Default.Equals(attributeSymbol.Symbol?.ContainingType, propertyAttributeType);
                });
            if (propertyAttribute is null) return;

            var activeParameterSyntax = activeNode.FirstAncestorOrSelf<ParameterSyntax>();
            if (activeParameterSyntax is null) return;

            var hasExistingMemberGenAttribute = activeParameterSyntax
                .AttributeLists
                .SelectMany(l => l.Attributes)
                .Any(a =>
                {
                    var attributeSymbol = semanticModel.GetSymbolInfo(a);
                    return SymbolEqualityComparer.Default.Equals(attributeSymbol.Symbol?.ContainingType, memberGenAttributeType);
                });
            if (hasExistingMemberGenAttribute) return;

            var compilation = semanticModel.Compilation;
            var genGenericType = compilation.GetTypeByMetadataName("GalaxyCheck.IGen`1");
            var parameterType = semanticModel.GetDeclaredSymbol(activeParameterSyntax)!.Type;
            var genType = genGenericType!.Construct(parameterType);

            context.RegisterRefactoring(new CodeActions.ConfigureGenWithMember(
                document,
                activeMethodDeclarationSyntax,
                activeParameterSyntax,
                genType,
                parameterType,
                specifyParameter: false));
        }
    }
}
