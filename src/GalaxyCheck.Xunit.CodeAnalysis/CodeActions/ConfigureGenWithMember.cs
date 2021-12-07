using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace GalaxyCheck.Xunit.CodeAnalysis.CodeActions
{
    public class ConfigureGenWithMember : CodeAction
    {
        private readonly Document _document;
        private readonly MethodDeclarationSyntax _methodDeclarationSyntax;
        private readonly ParameterSyntax _parameterSyntax;
        private readonly ITypeSymbol _genType;
        private readonly ITypeSymbol _parameterType;
        private readonly bool _specifyParameter;

        public override string Title => _specifyParameter
            ? "Configure generation of parameter with MemberGenAttribute"
            : "Configure generation of parameter with MemberGenAttribute";

        public ConfigureGenWithMember(
            Document document,
            MethodDeclarationSyntax methodDeclarationSyntax,
            ParameterSyntax parameterSyntax,
            ITypeSymbol genType,
            ITypeSymbol parameterType,
            bool specifyParameter)
        {
            _document = document;
            _methodDeclarationSyntax = methodDeclarationSyntax;
            _parameterSyntax = parameterSyntax;
            _genType = genType;
            _parameterType = parameterType;
            _specifyParameter = specifyParameter;
        }

        protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
        {
            var editor = await DocumentEditor.CreateAsync(_document, cancellationToken);
            var syntaxGenerator = SyntaxGenerator.GetGenerator(_document);

            var memberName = $"{_methodDeclarationSyntax.Identifier.Text}_{_parameterSyntax.Identifier.Text}";

            var propertySyntax = CreateGenPropertyDeclaration(syntaxGenerator, memberName, _genType, _parameterType, _parameterSyntax);
            var attributeSyntax = CreateMemberGenAttribute(syntaxGenerator, memberName);

            editor.InsertBefore(_methodDeclarationSyntax, propertySyntax);
            editor.ReplaceNode(_parameterSyntax, _parameterSyntax.AddAttributeLists(attributeSyntax));
            return editor.GetChangedDocument();
        }

        private static PropertyDeclarationSyntax CreateGenPropertyDeclaration(
            SyntaxGenerator syntaxGenerator,
            string name,
            ITypeSymbol genType,
            ITypeSymbol parameterType,
            ParameterSyntax parameterSyntax)
        {
            var propertyType = syntaxGenerator.TypeExpression(genType);

            var propertyExpression = (parameterType.ContainingSymbol.Name, parameterType.Name) switch
            {
                ("System", "Boolean") => SimpleBuiltInGenInvocation("Boolean"),
                ("System", "Byte") => SimpleBuiltInGenInvocation("Byte"),
                ("System", "Int16") => SimpleBuiltInGenInvocation("Int16"),
                ("System", "Int32") => SimpleBuiltInGenInvocation("Int32"),
                ("System", "Int64") => SimpleBuiltInGenInvocation("Int64"),
                ("System", "Char") => SimpleBuiltInGenInvocation("Char"),
                ("System", "String") => SimpleBuiltInGenInvocation("String"),
                ("System", "DateTime") => SimpleBuiltInGenInvocation("DateTime"),
                ("System", "Guid") => SimpleBuiltInGenInvocation("Guid"),
                _ => CreateGenInvocation(parameterType, parameterSyntax)
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

        private static InvocationExpressionSyntax CreateGenInvocation(ITypeSymbol parameterType, ParameterSyntax parameterSyntax)
        {
            return InvocationExpression(
                MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Gen"),
                    GenericName(
                        Identifier("Create"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList(parameterSyntax.Type!)))));
        }

        private static AttributeListSyntax CreateMemberGenAttribute(
            SyntaxGenerator syntaxGenerator,
            string memberName) => (AttributeListSyntax)syntaxGenerator.Attribute(
                "MemberGen",
                syntaxGenerator.NameOfExpression(syntaxGenerator.IdentifierName(memberName)));
    }
}
