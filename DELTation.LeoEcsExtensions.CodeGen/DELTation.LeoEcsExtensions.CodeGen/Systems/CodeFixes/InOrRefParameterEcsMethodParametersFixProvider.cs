using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(InOrRefParameterEcsMethodParametersFixProvider))]
    [Shared]
    public class InOrRefParameterEcsMethodParametersFixProvider : CodeFixProvider
    {
        private const string AddInModifierTitle = "Add in modifier";
        private const string AddRefModifierTTitle = "Add ref modifier";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(EcsRunParametersAnalyzer.InOrRefParameterDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;


            context.RegisterCodeFix(
                CodeAction.Create(AddInModifierTitle, c =>
                        FixModifiersAsync(context.Document, root!, diagnosticSpan, SyntaxKind.InKeyword),
                    AddInModifierTitle
                ), diagnostic
            );
            context.RegisterCodeFix(
                CodeAction.Create(AddRefModifierTTitle, c =>
                        FixModifiersAsync(context.Document, root!, diagnosticSpan, SyntaxKind.RefKeyword),
                    AddRefModifierTTitle
                ), diagnostic
            );
        }

        private static Task<Document> FixModifiersAsync(Document document,
            SyntaxNode root, TextSpan diagnosticSpan, SyntaxKind newKeywordKind)
        {
            var parameterSyntax =
                root.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                    .OfType<ParameterSyntax>().First();
            var newParameterSyntax = parameterSyntax
                    .AddModifiers(SyntaxFactory.Token(newKeywordKind))
                ;
            root = root.ReplaceNode(parameterSyntax, newParameterSyntax);

            var newDocument = document.WithSyntaxRoot(root);
            return Task.FromResult(newDocument);
        }
    }
}