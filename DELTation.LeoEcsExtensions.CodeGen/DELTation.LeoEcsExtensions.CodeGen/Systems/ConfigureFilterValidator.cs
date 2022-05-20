using System;
using Microsoft.CodeAnalysis;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.ConfigureFilterValidator.Result;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems
{
    public class ConfigureFilterValidator
    {
        public enum Result
        {
            Valid,
            InvalidName,
            NonVoidReturnType,
            InvalidParametersCount,
            InvalidParameterType,
        }

        public Result Run(IMethodSymbol methodSymbol)
        {
            if (methodSymbol == null) throw new ArgumentNullException(nameof(methodSymbol));
            if (!methodSymbol.IsAnyOfConfigureFilterMethods())
                return InvalidName;

            if (methodSymbol.ReturnType.ToString() != "void")
                return NonVoidReturnType;

            var parameterList = methodSymbol.Parameters;
            if (parameterList.Length != 1)
                return InvalidParametersCount;

            var parameter = parameterList[0];
            var parameterTypeName = parameter.Type is INamedTypeSymbol parameterType
                ? parameterType.GetFullyQualifiedName()
                : null;
            if (parameterTypeName != "Leopotam.EcsLite.EcsWorld+Mask")
                return InvalidParameterType;

            return Valid;
        }
    }
}