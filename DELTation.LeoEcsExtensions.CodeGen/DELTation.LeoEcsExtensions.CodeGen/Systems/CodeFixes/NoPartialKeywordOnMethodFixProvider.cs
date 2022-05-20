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
using Microsoft.CodeAnalysis.Formatting;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(NoPartialKeywordOnMethodFixProvider)), Shared]
	public class NoPartialKeywordOnMethodFixProvider : CodeFixProvider
	{
		private const string Title = "Add partial keyword to method";

		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(NoPartialKeywordOnMethodAnalyzer.NoPartialKeywordId);

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
				CodeAction.Create(Title, c =>
						FixAsync(context.Document, mds, c), Title
				), diagnostic
			);
		}

		private async Task<Document> FixAsync(Document document,
			MethodDeclarationSyntax mds,
			CancellationToken cancellationToken)
		{
			var newMds = mds
					.WithModifiers(TokenList(mds.Modifiers.Where(m =>
								!m.IsAccessModifierKeyword()
							)
							.Append(Token(SyntaxKind.PartialKeyword))
						)
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