using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class EcsMethodReturnTypeAnalyzer : DiagnosticAnalyzer
	{
		public const string EcsMethodReturnTypeId = "LEOECS014";

		private static readonly DiagnosticDescriptor EcsMethodReturnType = new DiagnosticDescriptor(
			EcsMethodReturnTypeId,
			"Invalid return type for an ECS method",
			"ECS methods only allowed to return void/bool but the return type is {0}",
			DiagnosticCategory,
			DiagnosticSeverity.Error,
			true
		);

		public override ImmutableArray<DiagnosticDescriptor>
			SupportedDiagnostics =>
			ImmutableArray.Create(EcsMethodReturnType);

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
			if (!mds.AttributeLists.HasAnyEcsMethodAttribute()) return;

			var returnTypeSyntax = mds.ReturnType;
			var returnType = context.SemanticModel.GetType(returnTypeSyntax);
			var returnTypeFullyQualified = returnType.GetFullyQualifiedName();
			if (returnTypeFullyQualified == VoidFullyQualifiedName ||
			    returnTypeFullyQualified == BoolFullyQualifiedName) return;

			context.ReportDiagnostic(Diagnostic.Create(
					EcsMethodReturnType,
					returnTypeSyntax.GetLocation(),
					returnTypeSyntax.ToString()
				)
			);
		}
	}
}