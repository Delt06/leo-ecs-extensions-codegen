using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DuplicateEcsMethodAnalyzer : DiagnosticAnalyzer
    {
        private const string DuplicateEcsMethodId = "LEOECS013";

        private static readonly DiagnosticDescriptor DuplicateEcsMethod = new DiagnosticDescriptor(
            DuplicateEcsMethodId,
            "Duplicate ECS method",
            "Maximum one of each ECS methods is allowed",
            Constants.DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(DuplicateEcsMethod);

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
            if (!cds.GetMethodsWithAnyEcsMethodAttribute().Any()) return;

            var methods = cds.Members.OfType<MethodDeclarationSyntax>()
                .ToList();
            var runMethods = new List<MethodDeclarationSyntax>();
            var initMethods = new List<MethodDeclarationSyntax>();
            var destroyMethods = new List<MethodDeclarationSyntax>();
            methods.ForEach(m =>
                {
                    var list = m.SwitchOnEcsMethodAttribute<List<MethodDeclarationSyntax>?>(
                        () => runMethods,
                        () => initMethods,
                        () => destroyMethods,
                        () => null
                    );
                    list?.Add(m);
                }
            );

            TryReportDiagnostic(context, runMethods);
            TryReportDiagnostic(context, initMethods);
            TryReportDiagnostic(context, destroyMethods);
        }

        private static void TryReportDiagnostic(SyntaxNodeAnalysisContext context,
            List<MethodDeclarationSyntax> methods)
        {
            if (methods.Count <= 1) return;

            foreach (var method in methods)
            {
                context.ReportDiagnostic(Diagnostic.Create(DuplicateEcsMethod,
                        method.Identifier.GetLocation()
                    )
                );
            }
        }
    }
}