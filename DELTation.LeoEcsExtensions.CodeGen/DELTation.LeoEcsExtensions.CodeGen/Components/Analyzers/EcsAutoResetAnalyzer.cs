using System.Collections.Immutable;
using System.Text.RegularExpressions;
using DELTation.LeoEcsExtensions.CodeGen.Systems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Components.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EcsAutoResetAnalyzer : DiagnosticAnalyzer
    {
        public const string EcsAutoResetTypeParameterId = "LEOECS104";

        private static readonly DiagnosticDescriptor EcsAutoResetTypeParameter = new DiagnosticDescriptor(
            EcsAutoResetTypeParameterId,
            "IEcsAutoReset<T> has incorrect type parameter",
            "The type parameter of implemented IEcsAutoReset<T> should be {0}",
            Constants.DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(EcsAutoResetTypeParameter);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.RegisterSyntaxNodeAction(
                AnalyzeNode, SyntaxKind.StructDeclaration
            );
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var sds = (StructDeclarationSyntax) context.Node;
            if (sds.BaseList == null)
                return;

            var semanticModel = context.SemanticModel;
            var structSymbol = semanticModel.GetType(sds);

            foreach (var baseType in sds.BaseList.Types)
            {
                if (!(baseType.Type is GenericNameSyntax genericTypeSyntax)) continue;

                var baseTypeSymbol = semanticModel.GetType(baseType.Type);
                var baseTypeFqName = baseTypeSymbol.GetFullyQualifiedName();
                if (!Regex.IsMatch(baseTypeFqName, @"^Leopotam\.EcsLite\.IEcsAutoReset<.*>$")) continue;

                var typeParameters = genericTypeSyntax.TypeArgumentList.Arguments;
                if (typeParameters.Count != 1) continue;

                var typeParameter = typeParameters[0];
                var typeParameterSymbol = semanticModel.GetType(typeParameter);
                if (structSymbol.GetFullyQualifiedName() == typeParameterSymbol.GetFullyQualifiedName()) continue;

                var location = typeParameter.GetLocation();
                context.ReportDiagnostic(Diagnostic.Create(
                        EcsAutoResetTypeParameter,
                        location,
                        sds.Identifier.ToString()
                    )
                );
            }
        }
    }
}