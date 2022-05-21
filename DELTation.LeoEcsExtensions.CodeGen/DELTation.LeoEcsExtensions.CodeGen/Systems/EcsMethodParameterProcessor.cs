using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.EcsMethodParameterProcessor.ExtraError;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.EcsMethodParameterProcessor.Result;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems
{
    public class EcsMethodParameterProcessor
    {
        public enum ExtraError
        {
            NoIncludes,
        }

        public enum Result
        {
            Pool,
            EntityId,
            EntityIdWithModifiers,
            Component,
            ComponentOfReferenceType,
            ComponentDuplicate,
            ComponentWithOutModifier,
            ComponentWithoutInOrRef,
            UnityEngineObject,
        }

        private readonly Compilation _compilation;

        private readonly SemanticModel _semanticModel;

        public EcsMethodParameterProcessor(SemanticModel semanticModel, Compilation compilation)
        {
            _semanticModel = semanticModel;
            _compilation = compilation;
        }

        public (Result[] results, ExtraError[] extraErrors) Run(IReadOnlyList<ParameterSyntax> parameters)
        {
            var results = new Result[parameters.Count];
            var componentTypeNames = new HashSet<string>();

            for (var index = 0; index < parameters.Count; index++)
            {
                var parameter = parameters[index];
                results[index] = Run(parameter, componentTypeNames);
            }

            var extraErrors = new List<ExtraError>();

            if (componentTypeNames.Count == 0)
                extraErrors.Add(NoIncludes);

            return (results, extraErrors.ToArray());
        }

        private Result Run(ParameterSyntax parameter, HashSet<string> componentTypeNames)
        {
            var parameterTypeSymbol = _semanticModel.GetType(parameter);
            if (parameterTypeSymbol.IsDescendantOfUnityEngineObject())
                return UnityEngineObject;

            var parameterTypeName = parameterTypeSymbol.GetFullyQualifiedName();


            var parameterModifiers = parameter.Modifiers;

            if (parameterTypeName.StartsWith("Leopotam.EcsLite.EcsPool<") &&
                parameterTypeSymbol.TypeArguments.Length == 1)
                return Pool;

            const string idTypeName = "System.Int32";

            if (parameterTypeName == idTypeName)
            {
                if (parameterModifiers.Any()) return EntityIdWithModifiers;

                return EntityId;
            }

            var componentType = _compilation.GetTypeByMetadataName(parameterTypeName);
            if (componentType is { IsValueType: false })
                return ComponentOfReferenceType;

            if (componentTypeNames.Contains(parameterTypeName))
                return ComponentDuplicate;

            componentTypeNames.Add(parameterTypeName);

            var hasIn = parameterModifiers.Any(SyntaxKind.InKeyword);
            var hasRef = parameterModifiers.Any(SyntaxKind.RefKeyword);
            var hasOut = parameterModifiers.Any(SyntaxKind.OutKeyword);

            if (hasOut)
                return ComponentWithOutModifier;

            if (!hasIn && !hasRef)
                return ComponentWithoutInOrRef;

            return Component;
        }
    }
}