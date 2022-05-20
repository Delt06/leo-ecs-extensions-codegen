using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class OneEcsMethodAttributeAnalyzer : DiagnosticAnalyzer
	{
		private const string OneEcsMethodAttributeId = "LEOECS012";

		private static readonly DiagnosticDescriptor OneEcsMethodAttribute = new DiagnosticDescriptor(
			OneEcsMethodAttributeId,
			"Multiple ECS method attributes",
			"Only one ECS method attribute is allowed at a time",
			Constants.DiagnosticCategory,
			DiagnosticSeverity.Error,
			true
		);

		public override ImmutableArray<DiagnosticDescriptor>
			SupportedDiagnostics =>
			ImmutableArray.Create(OneEcsMethodAttribute);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
			                                       GeneratedCodeAnalysisFlags.ReportDiagnostics
			);
			context.RegisterSyntaxNodeAction(
				AnalyzeNode, SyntaxKind.MethodDeclaration
			);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			var mds = (MethodDeclarationSyntax)context.Node;

			var ecsMethodAttributes = mds
				.AttributeLists.SelectMany(al => al.Attributes)
				.Where(a => a.IsAnyEcsMethodAttribute());
			if (ecsMethodAttributes.Count() > 1)
				context.ReportDiagnostic(Diagnostic.Create(OneEcsMethodAttribute,
						Location.Create(mds.SyntaxTree, mds.AttributeLists.FullSpan)
					)
				);
		}
	}
}