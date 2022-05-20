using System;
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
    public class EcsRunParametersAnalyzer : DiagnosticAnalyzer
    {
        public const string OutParameterDiagnosticId = "LEOECS001";
        public const string EntityIdModifiersDiagnosticId = "LEOECS002";
        public const string InOrRefParameterDiagnosticId = "LEOECS003";
        public const string ReferenceTypeParameterDiagnosticId = "LEOECS004";
        public const string DuplicateParameterTypeDiagnosticId = "LEOECS005";
        public const string NoIncludesId = "LEOECS007";

        private static readonly DiagnosticDescriptor OutParameterDiagnostic = new DiagnosticDescriptor(
            OutParameterDiagnosticId,
            "Out modifier",
            "Formal parameter {0} has out modifier",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor EntityIdModifiersDiagnostic = new DiagnosticDescriptor(
            EntityIdModifiersDiagnosticId,
            "Modifiers for id",
            "Formal parameter {0} is not supposed to have modifiers because it is entity id",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor InOrRefParameterDiagnostic = new DiagnosticDescriptor(
            InOrRefParameterDiagnosticId,
            "In or Ref modifier",
            "Formal parameter {0} is required to have in or ref modifier",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor ReferenceTypeParameterDiagnostic = new DiagnosticDescriptor(
            ReferenceTypeParameterDiagnosticId,
            "Reference type parameter",
            "Formal parameter {0} is required to be of a value type",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor DuplicateParameterTypeDiagnostic = new DiagnosticDescriptor(
            DuplicateParameterTypeDiagnosticId,
            "Duplicate parameter type",
            "Formal parameter {0} is a duplicate type ({1})",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        private static readonly DiagnosticDescriptor NoIncludes = new DiagnosticDescriptor(
            NoIncludesId,
            "No components found",
            "At least one component type (include) is required",
            DiagnosticCategory,
            DiagnosticSeverity.Error,
            true
        );

        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics =>
            ImmutableArray.Create(OutParameterDiagnostic, DuplicateParameterTypeDiagnostic,
                ReferenceTypeParameterDiagnostic, EntityIdModifiersDiagnostic, InOrRefParameterDiagnostic, NoIncludes
            );

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
            var mds = (MethodDeclarationSyntax) context.Node;
            if (!mds.AttributeLists.HasAnyEcsMethodAttribute()) return;

            var parameterProcessor = new EcsMethodParameterProcessor(context.SemanticModel, context.Compilation);
            var parameters = mds.ParameterList.Parameters;
            var (results, extraErrors) = parameterProcessor.Run(parameters);

            for (var index = 0; index < results.Length; index++)
            {
                var result = results[index];
                var parameter = parameters[index];

                switch (result)
                {
                    case EcsMethodParameterProcessor.Result.EntityIdWithModifiers:
                        context.ReportDiagnostic(Diagnostic.Create(EntityIdModifiersDiagnostic,
                                parameter.GetLocation(),
                                parameter.Identifier.ToString()
                            )
                        );
                        break;
                    case EcsMethodParameterProcessor.Result.ComponentOfReferenceType:
                        context.ReportDiagnostic(Diagnostic.Create(ReferenceTypeParameterDiagnostic,
                                parameter.Type?.GetLocation(),
                                parameter.Identifier.ToString()
                            )
                        );
                        break;
                    case EcsMethodParameterProcessor.Result.ComponentDuplicate:
                        context.ReportDiagnostic(Diagnostic.Create(DuplicateParameterTypeDiagnostic,
                                parameter.GetLocation(),
                                parameter.Identifier.ToString(),
                                parameter.Type?.ToString()
                            )
                        );
                        break;
                    case EcsMethodParameterProcessor.Result.ComponentWithOutModifier:
                        context.ReportDiagnostic(Diagnostic.Create(OutParameterDiagnostic, parameter.GetLocation(),
                                parameter.Identifier.ToString()
                            )
                        );
                        break;
                    case EcsMethodParameterProcessor.Result.ComponentWithoutInOrRef:
                        context.ReportDiagnostic(Diagnostic.Create(InOrRefParameterDiagnostic,
                                parameter.GetLocation(),
                                parameter.Identifier.ToString()
                            )
                        );
                        break;

                    case EcsMethodParameterProcessor.Result.Pool:
                    case EcsMethodParameterProcessor.Result.EntityId:
                    case EcsMethodParameterProcessor.Result.Component:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                foreach (var extraError in extraErrors)
                {
                    switch (extraError)
                    {
                        case EcsMethodParameterProcessor.ExtraError.NoIncludes:
                            context.ReportDiagnostic(Diagnostic.Create(NoIncludes, mds.ParameterList.GetLocation()));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }
    }
}