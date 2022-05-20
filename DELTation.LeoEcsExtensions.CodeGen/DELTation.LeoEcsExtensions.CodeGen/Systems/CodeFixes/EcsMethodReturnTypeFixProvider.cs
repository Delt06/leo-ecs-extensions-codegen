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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.CodeFixes
{
	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(EcsMethodReturnTypeFixProvider)), Shared]
	public class EcsMethodReturnTypeFixProvider : CodeFixProvider
	{
		private const string VoidTitle = "Make method return 'void'";
		private const string BoolTitle = "Make method return 'bool'";

		public sealed override ImmutableArray<string> FixableDiagnosticIds =>
			ImmutableArray.Create(EcsMethodReturnTypeAnalyzer.EcsMethodReturnTypeId);

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
				CodeAction.Create(VoidTitle, c =>
						FixAsync(context.Document, mds, c, "void"), VoidTitle
				), diagnostic
			);
			context.RegisterCodeFix(
				CodeAction.Create(BoolTitle, c =>
						FixAsync(context.Document, mds, c, "bool"), BoolTitle
				), diagnostic
			);
		}

		private static async Task<Document> FixAsync(Document document,
			MethodDeclarationSyntax mds,
			CancellationToken cancellationToken, string newType)
		{
			var newReturnType = PredefinedType(ParseToken(newType))
					.WithTriviaFrom(mds.ReturnType)
				;
			var newMds = mds.WithReturnType(newReturnType);
			var root = await document.GetSyntaxRootAsync(cancellationToken);
			root = root!.ReplaceNode(mds, newMds);

			var newDocument = document.WithSyntaxRoot(root);

			return newDocument;
		}
	}
}