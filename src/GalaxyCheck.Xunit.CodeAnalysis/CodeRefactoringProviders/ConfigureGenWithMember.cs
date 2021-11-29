using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
            if (activeParameterSyntax is not null)
            {
                context.RegisterRefactoring(InsertAndReferenceMemberGen(
                    semanticModel,
                    document,
                    activeMethodDeclarationSyntax,
                    activeParameterSyntax));
            }
        }

        private static CodeAction InsertAndReferenceMemberGen(
            SemanticModel semanticModel,
            Document document,
            MethodDeclarationSyntax methodDeclarationSyntax,
            ParameterSyntax parameterSyntax)
        {
            var compilation = semanticModel.Compilation;
            var title = "Configure generation of parameter with MemberGenAttribute";

            var genGenericType = compilation.GetTypeByMetadataName("GalaxyCheck.IGen`1");
            var parameterType = semanticModel.GetDeclaredSymbol(parameterSyntax)!.Type;
            var genType = genGenericType!.Construct(parameterType);

            var genFactoryType = compilation.GetTypeByMetadataName("GalaxyCheck.Gen")!;

            return CodeAction.Create(title, async cancellationToken =>
            {
                var editor = await DocumentEditor.CreateAsync(document, cancellationToken);
                var syntaxGenerator = SyntaxGenerator.GetGenerator(document);

                var memberName = $"{methodDeclarationSyntax.Identifier.Text}_{parameterSyntax.Identifier.Text}";

                var propertySyntax = CreateProperty(syntaxGenerator, memberName, genType, parameterType);
                var attributeSyntax = CreateMemberGenAttribute(syntaxGenerator, memberName);

                editor.InsertBefore(methodDeclarationSyntax, propertySyntax);
                editor.ReplaceNode(parameterSyntax, parameterSyntax.AddAttributeLists(attributeSyntax));
                return editor.GetChangedDocument();
            });
        }

        private static PropertyDeclarationSyntax CreateProperty(SyntaxGenerator syntaxGenerator, string name, ITypeSymbol genType, ITypeSymbol parameterType)
        {
            var propertyType = syntaxGenerator.TypeExpression(genType);

            var propertyExpression = parameterType.Name switch
            {
                "Boolean" => SimpleBuiltInGenInvocation("Boolean"),
                "Byte" => SimpleBuiltInGenInvocation("Byte"),
                "Int16" => SimpleBuiltInGenInvocation("Int16"),
                "Int32" => SimpleBuiltInGenInvocation("Int32"),
                "Int64" => SimpleBuiltInGenInvocation("Int64"),
                "Char" => SimpleBuiltInGenInvocation("Char"),
                "String" => SimpleBuiltInGenInvocation("String"),
                "DateTime" => SimpleBuiltInGenInvocation("DateTime"),
                "Guid" => SimpleBuiltInGenInvocation("Guid"),
                _ =>  InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Gen"),
                        GenericName(
                            Identifier("Create"))
                        .WithTypeArgumentList(
                            TypeArgumentList(
                                SingletonSeparatedList<TypeSyntax>(IdentifierName(parameterType.Name))))))
            };

            var propertyValue = ArrowExpressionClause(propertyExpression);

            return ((PropertyDeclarationSyntax)syntaxGenerator.PropertyDeclaration(
                name,
                propertyType,
                Accessibility.Private,
                DeclarationModifiers.Static | DeclarationModifiers.ReadOnly,
                getAccessorStatements: null,
                setAccessorStatements: null))
                    .WithAccessorList(null)
                    .WithExpressionBody(propertyValue)
                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }

        private static InvocationExpressionSyntax SimpleBuiltInGenInvocation(string methodName)
        {
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Gen"),
                    IdentifierName(methodName)));
        }

        private static AttributeListSyntax CreateMemberGenAttribute(SyntaxGenerator syntaxGenerator, string memberName) => (AttributeListSyntax)syntaxGenerator.Attribute(
            "MemberGen",
            syntaxGenerator.NameOfExpression(syntaxGenerator.IdentifierName(memberName)));
    }
}
