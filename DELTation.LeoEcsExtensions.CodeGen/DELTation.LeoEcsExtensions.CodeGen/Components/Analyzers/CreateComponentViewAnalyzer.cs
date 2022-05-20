using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Components.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class CreateComponentViewAnalyzer : DiagnosticAnalyzer
	{
		public const string CreateComponentViewId = "LEOECS101";

		private static readonly DiagnosticDescriptor CreateComponentView = new DiagnosticDescriptor(
			CreateComponentViewId,
			"Create a ComponentView",
			"Create a ComponentView for {0}",
			DiagnosticCategory,
			DiagnosticSeverity.Info,
			true
		);

		public override ImmutableArray<DiagnosticDescriptor>
			SupportedDiagnostics =>
			ImmutableArray.Create(CreateComponentView);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
			                                       GeneratedCodeAnalysisFlags.ReportDiagnostics
			);
			context.RegisterSyntaxNodeAction(
				AnalyzeNode, SyntaxKind.StructDeclaration
			);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			var sds = (StructDeclarationSyntax)context.Node;
			if (!sds.AttributeLists.HasEcsComponentAttribute()) return;
			if (sds.AttributeLists.HasEcsComponentWithViewAttribute()) return;

			context.ReportDiagnostic(Diagnostic.Create(CreateComponentView, sds.Identifier.GetLocation(),
					sds.GetIdentifierWithTypeParameters()
				)
			);
		}
	}
}