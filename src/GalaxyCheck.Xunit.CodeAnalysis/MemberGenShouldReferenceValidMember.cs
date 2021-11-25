using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Xunit.CodeAnalysis
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class MemberGenShouldReferenceValidMember : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            Descriptors.GCX1000_MemberGenMustReferenceExistingMember,
            Descriptors.GCX1001_MemberGenMustReferenceValidMemberType,
            Descriptors.GCX1002_MemberGenMustReferenceMemberOfValidType);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);

            context.RegisterCompilationStartAction(AnalyzeCompilation);
        }

        private static void AnalyzeCompilation(CompilationStartAnalysisContext compilationContext)
        {
            compilationContext.RegisterSyntaxNodeAction(
                syntaxNodeAnalysisContext => AnalyzeAttribute(compilationContext.Compilation, syntaxNodeAnalysisContext),
                SyntaxKind.Attribute);
        }

        private static void AnalyzeAttribute(Compilation compilation, SyntaxNodeAnalysisContext context)
        {
            var memberGenAttributeType = compilation.GetTypeByMetadataName("GalaxyCheck.MemberGenAttribute");
            if (memberGenAttributeType == null)
            {
                return;
            }

            var genGenericType = compilation.GetTypeByMetadataName("GalaxyCheck.IGen`1");
            if (genGenericType == null)
            {
                return;
            }

            if (context.Node is not AttributeSyntax attribute)
            {
                return;
            }


            var semanticModel = context.SemanticModel;
            if (!SymbolEqualityComparer.Default.Equals(semanticModel!.GetTypeInfo(attribute, context.CancellationToken).Type, memberGenAttributeType))
            {
                return;
            }

            var memberNameArgument = attribute.ArgumentList!.Arguments.First();
            var constantValue = semanticModel.GetConstantValue(memberNameArgument.Expression, context.CancellationToken);
            if (constantValue.Value is not string memberName)
            {
                return;
            }

            var parameter = attribute.FirstAncestorOrSelf<ParameterSyntax>();
            if (parameter is null)
            {
                return;
            }

            var testClassTypeSymbol = semanticModel.GetDeclaredSymbol(attribute.FirstAncestorOrSelf<ClassDeclarationSyntax>()!)!;
            var memberSymbol = FindMemberSymbol(memberName, testClassTypeSymbol);

            if (memberSymbol is null)
            {
                ReportMissingMember(context, attribute, memberName, testClassTypeSymbol);
                return;
            }

            if (memberSymbol.Kind != SymbolKind.Property && memberSymbol.Kind != SymbolKind.Field)
            {
                ReportIncorrectMemberType(context, attribute);
                return;
            }

            var parameterType = semanticModel.GetDeclaredSymbol(parameter)!.Type;
            if (parameterType is null)
            {
                return;
            }

            var genType = genGenericType.Construct(parameterType);
            var memberType = GetMemberType(memberSymbol);
            if (!genType.IsAssignableFrom(memberType))
            {
                ReportIncorrectReturnType(context, attribute, genType, memberType);
                return;
            }
        }

        private static ISymbol? FindMemberSymbol(string memberName, ITypeSymbol? type)
        {
            while (type is not null)
            {
                var memberSymbol = type.GetMembers(memberName).FirstOrDefault();
                if (memberSymbol is not null)
                    return memberSymbol;

                type = type.BaseType;
            }

            return null;
        }

        private static ITypeSymbol GetMemberType(ISymbol memberSymbol) =>
            memberSymbol switch
            {
                IPropertySymbol prop => prop.Type,
                IFieldSymbol field => field.Type,
                _ => throw new NotSupportedException("Invalid member type"),
            };

        private static void ReportMissingMember(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            string memberName,
            ITypeSymbol declaredMemberTypeSymbol) =>
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.GCX1000_MemberGenMustReferenceExistingMember,
                        attribute.GetLocation(),
                        memberName,
                        SymbolDisplay.ToDisplayString(declaredMemberTypeSymbol)
                    )
                );

        private static void ReportIncorrectMemberType(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute) =>
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.GCX1001_MemberGenMustReferenceValidMemberType,
                        attribute.GetLocation()
                    )
                );

        private static void ReportIncorrectReturnType(
            SyntaxNodeAnalysisContext context,
            AttributeSyntax attribute,
            INamedTypeSymbol expectedType,
            ITypeSymbol memberType) =>
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Descriptors.GCX1002_MemberGenMustReferenceMemberOfValidType,
                        attribute.GetLocation(),
                        SymbolDisplay.ToDisplayString(expectedType),
                        SymbolDisplay.ToDisplayString(memberType)
                    )
                );
    }
}
