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
    public class CreateConfigureRunFilterAnalyzer : DiagnosticAnalyzer
    {
        public const string ConfigureFilterId = "LEOECS015";

        private static readonly DiagnosticDescriptor ConfigureFilter = new DiagnosticDescriptor(
            ConfigureFilterId,
            "Create filter configuration method",
            "Create filter configuration method",
            DiagnosticCategory,
            DiagnosticSeverity.Info,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(ConfigureFilter);

        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
                                                   GeneratedCodeAnalysisFlags.ReportDiagnostics
            );
            context.RegisterSyntaxNodeAction(
                AnalyzeNode, SyntaxKind.Attribute
            );
        }

        private void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var attributeSyntax = (AttributeSyntax) context.Node;
            if (!attributeSyntax.IsAnyEcsMethodAttribute()) return;
            if (!SyntaxNodeHelper.TryGetParentSyntax(attributeSyntax, out ClassDeclarationSyntax cds)) return;

            var requiredMethodName = attributeSyntax.GetConfigureFilterMethodNameOrDefault();
            if (requiredMethodName == null) return;

            if (cds.Members.Any(m => m is MethodDeclarationSyntax mds && mds.Identifier.ToString() == requiredMethodName
                )) return;

            context.ReportDiagnostic(Diagnostic.Create(ConfigureFilter, attributeSyntax.GetLocation()));
        }
    }
}