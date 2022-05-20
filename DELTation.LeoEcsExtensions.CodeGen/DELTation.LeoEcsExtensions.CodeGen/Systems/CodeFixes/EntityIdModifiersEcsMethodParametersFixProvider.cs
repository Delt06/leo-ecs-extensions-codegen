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
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EntityIdModifiersEcsMethodParametersFixProvider)),
	 Shared]
	public class EntityIdModifiersEcsMethodParametersFixProvider : CodeFixProvider
	{
		private const string Title = "Remove all modifiers";

		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(EcsRunParametersAnalyzer.EntityIdModifiersDiagnosticId);

		public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
		{
			var root =
				await context.Document.GetSyntaxRootAsync(context.CancellationToken)
					.ConfigureAwait(false);

			var diagnostic = context.Diagnostics.First();
			var diagnosticSpan = diagnostic.Location.SourceSpan;


			context.RegisterCodeFix(
				CodeAction.Create(Title, c =>
						FixIdModifiersAsync(context.Document, root!, diagnosticSpan),
					Title
				), diagnostic
			);
		}

		private static Task<Document> FixIdModifiersAsync(Document document,
			SyntaxNode root, TextSpan diagnosticSpan)
		{
			var parameterSyntax =
				root.FindToken(diagnosticSpan.Start).Parent!.AncestorsAndSelf()
					.OfType<ParameterSyntax>().First();
			var newParameterSyntax = parameterSyntax
					.WithModifiers(SyntaxFactory.TokenList())
				;
			root = root.ReplaceNode(parameterSyntax, newParameterSyntax);

			var newDocument = document.WithSyntaxRoot(root);
			return Task.FromResult(newDocument);
		}
	}
}