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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RefReadonlyReturnFixProvider)), Shared]
	public class RefReadonlyReturnFixProvider : CodeFixProvider
	{
		private const string Title = "Add ref initializer";

		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(RefReadonlyReturnAnalyzer.RefReadonlyReturnId);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root =
				await context.Document.GetSyntaxRootAsync(context.CancellationToken)
					.ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;

			var ies =
				root!.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
					.OfType<InvocationExpressionSyntax>().First();
			context.RegisterCodeFix(
				CodeAction.Create(Title, c =>
						FixAsync(context.Document, ies, c), Title
				), diagnostic
			);
		}

		private async Task<Document> FixAsync(Document document,
			InvocationExpressionSyntax ies,
			CancellationToken cancellationToken)
		{
			var newExpression = SyntaxFactory.RefExpression(ies.Expression);
			var root = (await document.GetSyntaxRootAsync(cancellationToken))!;
			root = root.ReplaceNode(ies.Expression, newExpression );
			var newDocument = document.WithSyntaxRoot(root);

			return newDocument;
		}
	}
}