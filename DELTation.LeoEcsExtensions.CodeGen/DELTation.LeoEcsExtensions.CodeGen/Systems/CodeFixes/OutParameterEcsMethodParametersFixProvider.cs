using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers.EcsRunParametersAnalyzer;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(OutParameterEcsMethodParametersFixProvider))] [Shared]
    public class OutParameterEcsMethodParametersFixProvider : CodeFixProvider
    {
        private const string OutToInParameterTitle = "Change out to in";
        private const string OutToRefParameterTitle = "Remove out to ref";

        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(OutParameterDiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root =
                await context.Document.GetSyntaxRootAsync(context.CancellationToken)
                    .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;


            context.RegisterCodeFix(
                CodeAction.Create(OutToInParameterTitle, c =>
                        FixModifierAsync(context.Document, root!, diagnosticSpan, SyntaxKind.InKeyword),
                    OutToInParameterTitle
                ), diagnostic
            );
            context.RegisterCodeFix(
                CodeAction.Create(OutToRefParameterTitle, c =>
                        FixModifierAsync(context.Document, root!, diagnosticSpan, SyntaxKind.RefKeyword),
                    OutToRefParameterTitle
                ), diagnostic
            );
        }

        private static Task<Document> FixModifierAsync(Document document,
            SyntaxNode root, TextSpan diagnosticSpan, SyntaxKind newKeywordKind)
        {
            var parameterSyntax =
                root.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
                    .OfType<ParameterSyntax>().First();
            var newParameterSyntax = parameterSyntax
                    .WithModifiers(TokenList(parameterSyntax.Modifiers.Where(m => !m.IsKind(SyntaxKind.OutKeyword))))
                    .AddModifiers(Token(newKeywordKind))
                ;
            root = root.ReplaceNode(parameterSyntax, newParameterSyntax);

            var newDocument = document.WithSyntaxRoot(root);
            return Task.FromResult(newDocument);
        }
    }
}