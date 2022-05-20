using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

#pragma warning disable RS2008

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public class ConfigureFilterSignatureAnalyzer : DiagnosticAnalyzer
	{
		public const string ConfigureRunFilterParametersCountId = "LEOECS009";
		public const string ConfigureRunFilterParameterTypeId = "LEOECS010";
		public const string ConfigureRunFilterVoidReturnId = "LEOECS011";

		private static readonly DiagnosticDescriptor ConfigureFilterParametersCount = new DiagnosticDescriptor(
			ConfigureRunFilterParametersCountId,
			"Configure filter method incorrect number of parameters",
			"{0} method should have 1 parameter but has {1}",
			DiagnosticCategory,
			DiagnosticSeverity.Error,
			true
		);

		private static readonly DiagnosticDescriptor ConfigureFilterParameterType = new DiagnosticDescriptor(
			ConfigureRunFilterParameterTypeId,
			"Configure filter method incorrect parameter type",
			"{0} method parameter should have EcsWorld.Mask type but has {1}",
			DiagnosticCategory,
			DiagnosticSeverity.Error,
			true
		);

		private static readonly DiagnosticDescriptor ConfigureFilterVoidReturn = new DiagnosticDescriptor(
			ConfigureRunFilterVoidReturnId,
			"Configure filter method non-void return type",
			"{0} method should have void return type but has {1}",
			DiagnosticCategory,
			DiagnosticSeverity.Error,
			true
		);

		public override ImmutableArray<DiagnosticDescriptor>
			SupportedDiagnostics =>
			ImmutableArray.Create(ConfigureFilterParametersCount, ConfigureFilterParameterType,
				ConfigureFilterVoidReturn
			);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze |
			                                       GeneratedCodeAnalysisFlags.ReportDiagnostics
			);
			context.RegisterSyntaxNodeAction(
				AnalyzeNode, SyntaxKind.ClassDeclaration
			);
		}

		private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
		{
			var cds = (ClassDeclarationSyntax)context.Node;
			if (!cds.GetMethodsWithAnyEcsMethodAttribute().Any()) return;

			var semanticModel = context.SemanticModel;
			var validator = new ConfigureFilterValidator();

			foreach (var member in cds.Members)
			{
				if (!(member is MethodDeclarationSyntax configureMds)) continue;


				var method = semanticModel.GetMethod(configureMds);
				var result = validator.Run(method);

				switch (result)
				{
					case ConfigureFilterValidator.Result.Valid:
						break;
					case ConfigureFilterValidator.Result.InvalidName:
						break;
					case ConfigureFilterValidator.Result.NonVoidReturnType:
						context.ReportDiagnostic(Diagnostic.Create(ConfigureFilterVoidReturn,
								configureMds.ReturnType.GetLocation(),
								method.Name, configureMds.ReturnType.ToString()
							)
						);
						break;
					case ConfigureFilterValidator.Result.InvalidParametersCount:
						context.ReportDiagnostic(Diagnostic.Create(ConfigureFilterParametersCount,
								configureMds.ParameterList.GetLocation(),
								method.Name, configureMds.ParameterList.Parameters.Count
							)
						);
						break;
					case ConfigureFilterValidator.Result.InvalidParameterType:
						var firstParameter = configureMds.ParameterList.Parameters[0];
						context.ReportDiagnostic(Diagnostic.Create(ConfigureFilterParameterType,
								firstParameter.Type?.GetLocation(),
								method.Name, firstParameter.Type?.ToString()
							)
						);
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
		}
	}
}