using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ConfigureFilterSignatureFixProvider))] [Shared]
    public class ConfigureFilterSignatureFixProvider : CodeFixProvider
    {
        private const string ReturnTypeTitle = "Change return type to void";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ConfigureFilterSignatureAnalyzer.ConfigureRunFilterVoidReturnId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var mds =
                root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                    .OfType<MethodDeclarationSyntax>().First();

            context.RegisterCodeFix(
                CodeAction.Create(ReturnTypeTitle, c =>
                        FixReturnTypeAsync(context.Document, mds, c), ReturnTypeTitle
                ), diagnostic
            );
        }

        private static async Task<Document> FixReturnTypeAsync(Document document,
            MethodDeclarationSyntax mds,
            CancellationToken cancellationToken)
        {
            var newMds = mds
                    .WithReturnType(PredefinedType(ParseToken("void "))
                    )
                    .WithoutAnnotations(Formatter.Annotation)
                ;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            root = root!.ReplaceNode(mds, newMds);

            var newDocument = document.WithSyntaxRoot(root);
            return newDocument;
        }
    }
}