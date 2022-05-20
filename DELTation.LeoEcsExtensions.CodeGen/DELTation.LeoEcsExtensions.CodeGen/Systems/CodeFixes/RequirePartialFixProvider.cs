using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RequirePartialFixProvider))] [Shared]
    public class RequirePartialFixProvider : CodeFixProvider
    {
        private const string Title = "Add partial keyword to class";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(RequirePartialAnalyzer.RequirePartialId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var cds =
                root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                    .OfType<ClassDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(Title, c =>
                        FixAsync(context.Document, cds, c), Title
                ), diagnostic
            );
        }

        private async Task<Document> FixAsync(Document document,
            ClassDeclarationSyntax cds,
            CancellationToken cancellationToken)
        {
            var newCds = cds.AddModifiers(SyntaxFactory.Token(SyntaxKind.PartialKeyword));
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            root = root!.ReplaceNode(cds, newCds);

            var newDocument = document.WithSyntaxRoot(root);

            return newDocument;
        }
    }
}