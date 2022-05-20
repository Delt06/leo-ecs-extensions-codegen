using System.Collections.Immutable;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Components.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ComponentViewFileNameAnalyzer : DiagnosticAnalyzer
    {
        private const string FileNameDoesNotMatchClassNameId = "LEOECS103";

        private static readonly DiagnosticDescriptor FileNameDoesNotMatchClassName = new DiagnosticDescriptor(
            FileNameDoesNotMatchClassNameId,
            "File name does not match class name",
            "File name should be the same as the class name but found file {0} and class {1}",
            DiagnosticCategory,
            DiagnosticSeverity.Warning,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(FileNameDoesNotMatchClassName);

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
            var @class = context.SemanticModel.GetType(cds);

            if (!IsAssignableToComponentView(@class)) return;
            if (@class.IsAbstract) return;
            if (@class.IsGenericType) return;

            var filePath = context.Node.SyntaxTree.FilePath;
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var className = cds.Identifier.ToString();
            if (fileName == className) return;

            context.ReportDiagnostic(Diagnostic.Create(
                    FileNameDoesNotMatchClassName,
                    cds.Identifier.GetLocation(),
                    fileName,
                    className
                )
            );
        }

        private static bool IsAssignableToComponentView(INamedTypeSymbol? namedTypeSymbol)
        {
            while (true)
            {
                if (namedTypeSymbol == null) return false;


                var fullyQualifiedName = namedTypeSymbol.GetFullyQualifiedName();

                var matchesComponentViewTypeName = Regex.IsMatch(fullyQualifiedName,
                    @"^DELTation\.LeoEcsExtensions\.Views\.Components\.ComponentView<.*>$"
                );
                if (matchesComponentViewTypeName) return true;

                namedTypeSymbol = namedTypeSymbol.BaseType;
            }
        }
    }
}