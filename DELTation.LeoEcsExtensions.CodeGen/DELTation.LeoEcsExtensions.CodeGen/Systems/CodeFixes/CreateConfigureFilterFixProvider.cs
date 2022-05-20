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
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CreateConfigureFilterFixProvider))] [Shared]
    public class CreateConfigureFilterFixProvider : CodeFixProvider
    {
        private const string RunTitle = "Create ConfigureRunFilter method";
        private const string InitTitle = "Create ConfigureInitFilter method";
        private const string DestroyTitle = "Create ConfigureDestroyFilter method";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(CreateConfigureRunFilterAnalyzer.ConfigureFilterId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var attributeSyntax =
                root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                    .OfType<AttributeSyntax>().First();

            var methodName = attributeSyntax.GetConfigureFilterMethodNameOrDefault();
            var title = attributeSyntax.SwitchOnEcsMethodAttribute<string?>(
                () => RunTitle,
                () => InitTitle,
                () => DestroyTitle,
                () => null
            );
            if (methodName == null || title == null) return;

            context.RegisterCodeFix(
                CodeAction.Create(title, c =>
                        FixAsync(context.Document, attributeSyntax, c, methodName), title
                ), diagnostic
            );
        }

        private async Task<Document> FixAsync(Document document,
            AttributeSyntax attributeSyntax,
            CancellationToken cancellationToken, string methodName)
        {
            if (!SyntaxNodeHelper.TryGetParentSyntax(attributeSyntax, out ClassDeclarationSyntax cds)) return document;

            var members = cds.Members;
            members = members.Insert(0,
                ParseMemberDeclaration(
                    $"private static void {methodName}(EcsWorld.Mask filter) {{}}"
                )!.WithAdditionalAnnotations(Formatter.Annotation)
            );
            var newCds = cds.WithMembers(members);

            var root = await document.GetSyntaxRootAsync(cancellationToken);
            root = root!.ReplaceNode(cds, newCds);
            root = root.TryAddUsingStatement("Leopotam.EcsLite");
            var newDocument = document.WithSyntaxRoot(root);

            return newDocument;
        }
    }
}