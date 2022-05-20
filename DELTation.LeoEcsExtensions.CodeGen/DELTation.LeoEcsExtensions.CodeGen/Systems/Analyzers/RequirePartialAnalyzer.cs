using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class RequirePartialAnalyzer : DiagnosticAnalyzer
    {
        public const string RequirePartialId = "LEOECS006";

        private static readonly DiagnosticDescriptor RequirePartial = new DiagnosticDescriptor(
            RequirePartialId,
            "Partial keyword required",
            "ECS method requires class {0} to have partial keyword",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(RequirePartial);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.RegisterSyntaxNodeAction(
                AnalyzeNode, SyntaxKind.ClassDeclaration
            );
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var cds = (ClassDeclarationSyntax) context.Node;
            if (cds.Modifiers.Any(SyntaxKind.PartialKeyword)) return;
            if (!cds.GetMethodsWithAnyEcsMethodAttribute().Any()) return;

            context.ReportDiagnostic(Diagnostic.Create(RequirePartial,
                    cds.Identifier.GetLocation(), cds.Identifier.ToString()
                )
            );
        }
    }
}