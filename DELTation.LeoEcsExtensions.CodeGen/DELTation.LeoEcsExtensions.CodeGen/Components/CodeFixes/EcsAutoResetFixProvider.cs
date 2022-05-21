using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DELTation.LeoEcsExtensions.CodeGen.Components.Analyzers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Components.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EcsAutoResetFixProvider))] [Shared]
    public class EcsAutoResetFixProvider : CodeFixProvider
    {
        private const string Title = "Fix IEcsAutoReset<T> type parameter";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(EcsAutoResetAnalyzer.EcsAutoResetTypeParameterId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var parent = root!.FindToken(diagnosticSpan.Start).Parent!;
            var typeSyntax = parent.AncestorsAndSelf()
                .OfType<TypeSyntax>().First();
            var structSyntax = parent.AncestorsAndSelf()
                .OfType<StructDeclarationSyntax>().First();
            context.RegisterCodeFix(
                CodeAction.Create(Title, c =>
                        FixAsync(context.Document, typeSyntax, structSyntax, c), Title
                ), diagnostic
            );
        }

        private async Task<Document> FixAsync(Document document,
            TypeSyntax typeSyntax,
            StructDeclarationSyntax structSyntax,
            CancellationToken cancellationToken)
        {
            var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
            var newTypeSyntax = SyntaxFactory.ParseTypeName(structSyntax.Identifier.ToString());

            var newStructSyntax = structSyntax.ReplaceNode(typeSyntax, newTypeSyntax);

            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            if (semanticModel != null)
            {
                var methods = newStructSyntax.Members.OfType<MethodDeclarationSyntax>();
                foreach (var mds in methods)
                {
                    if (mds.Identifier.ToString() != "AutoReset") continue;

                    var parameters = mds.ParameterList.Parameters;
                    if (parameters.Count != 1) continue;

                    var parameter = parameters[0];
                    if (parameter.Type == null) continue;

                    newStructSyntax = newStructSyntax.ReplaceNode(parameter.Type,
                        newTypeSyntax.WithTriviaFrom(parameter.Type)
                    );
                    break;
                }
            }

            root = root.ReplaceNode(structSyntax, newStructSyntax);
            var newDocument = document.WithSyntaxRoot(root);
            return newDocument;
        }
    }
}