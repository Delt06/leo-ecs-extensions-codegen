using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NoPartialKeywordOnMethodAnalyzer : DiagnosticAnalyzer
    {
        public const string NoPartialKeywordId = "LEOECS008";

        private static readonly DiagnosticDescriptor NoPartialKeywordOnMethod = new DiagnosticDescriptor(
            NoPartialKeywordId,
            "Method is missing partial keyword",
            "Add partial keyword to method {0} to increase performance",
            DiagnosticCategory,
            DiagnosticSeverity.Warning,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(NoPartialKeywordOnMethod);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.RegisterSyntaxNodeAction(
                AnalyzeNode, SyntaxKind.MethodDeclaration
            );
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var mds = (MethodDeclarationSyntax) context.Node;
            if (!mds.AttributeLists.HasAnyEcsMethodAttribute()) return;
            if (mds.Modifiers.Any(SyntaxKind.PartialKeyword)) return;

            var returnType = context.SemanticModel.GetType(mds.ReturnType);
            if (returnType.GetFullyQualifiedName() != VoidFullyQualifiedName) return;

            context.ReportDiagnostic(Diagnostic.Create(NoPartialKeywordOnMethod,
                    mds.Identifier.GetLocation(),
                    mds.Identifier.ToString()
                )
            );
        }
    }
}