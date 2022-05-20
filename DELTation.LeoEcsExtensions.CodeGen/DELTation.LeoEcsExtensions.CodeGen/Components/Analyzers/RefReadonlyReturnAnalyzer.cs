using System.Collections.Immutable;
using DELTation.LeoEcsExtensions.CodeGen.Systems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Components.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RefReadonlyReturnAnalyzer : DiagnosticAnalyzer
    {
        public const string RefReadonlyReturnId = "LEOECS102";

        private static readonly DiagnosticDescriptor RefReadonlyReturn = new DiagnosticDescriptor(
            RefReadonlyReturnId,
            "Add ref initializer",
            "Add ref initializer",
            Constants.DiagnosticCategory,
            DiagnosticSeverity.Info,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(RefReadonlyReturn);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.RegisterSyntaxNodeAction(
                AnalyzeNode, SyntaxKind.LocalDeclarationStatement
            );
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var lds = (LocalDeclarationStatementSyntax) context.Node;
            var type = lds.Declaration.Type;
            if (!(type is RefTypeSyntax refTypeSyntax)) return;
            if (refTypeSyntax.ReadOnlyKeyword == default) return;

            foreach (var variable in lds.Declaration.Variables)
            {
                var value = variable.Initializer?.Value;
                if (!(value is InvocationExpressionSyntax invocationExpressionSyntax)) continue;

                var declaredSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax.Expression).Symbol;
                if (!(declaredSymbol is IMethodSymbol { ReturnsByRefReadonly: true })) continue;

                context.ReportDiagnostic(Diagnostic.Create(RefReadonlyReturn, invocationExpressionSyntax.GetLocation(),
                        invocationExpressionSyntax
                    )
                );
            }
        }
    }
}