using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.EcsMethodExtensions;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Methods
{
    public abstract class EcsMethodGeneratorBase
    {
        private readonly INamedTypeSymbol _class;
        private readonly MethodDeclarationSyntax _mds;
        private readonly List<IEcsMethodParameter> _parameters = new List<IEcsMethodParameter>();
        private bool _addInlineAttribute;
        private bool _booleanReturnType;
        private bool _callConfigureFilter;

        protected EcsMethodGeneratorBase(MethodDeclarationSyntax mds,
            INamedTypeSymbol @class)
        {
            _mds = mds;
            _class = @class;
        }

        protected abstract string Prefix { get; }
        protected abstract string ConfigureFilterMethodName { get; }

        protected abstract string ImplementedMethodName { get; }

        public void Init(Dictionary<string, string> poolNames, SemanticModel semanticModel, Compilation compilation,
            ISet<string> attributes)
        {
            InitializeParameters(poolNames, semanticModel, compilation, attributes);
            CheckForConfigureFilterMethod();
            CheckInlining();

            _booleanReturnType = semanticModel.GetType(_mds.ReturnType).GetFullyQualifiedName()
                                 == BoolFullyQualifiedName;
        }

        private void InitializeParameters(Dictionary<string, string> poolNames, SemanticModel semanticModel,
            Compilation compilation, ISet<string> attributes)
        {
            var parameterProcessor = new EcsMethodParameterProcessor(semanticModel, compilation);
            var parameterSyntaxList = _mds.ParameterList.Parameters;
            var (results, _) = parameterProcessor.Run(parameterSyntaxList);

            for (var index = 0; index < results.Length; index++)
            {
                var result = results[index];
                var parameter = parameterSyntaxList[index];
                var parameterTypeSymbol = semanticModel.GetType(parameter);
                var parameterTypeName = parameterTypeSymbol.GetFullyQualifiedName();

                switch (result)
                {
                    case EcsMethodParameterProcessor.Result.Pool:
                    {
                        var poolComponentType = parameterTypeSymbol.TypeArguments[0].GetFullyQualifiedName();
                        var poolName = GetOrCreatePoolName(poolNames, poolComponentType);
                        _parameters.Add(new PoolEcsMethodParameter(poolName, poolComponentType));
                        break;
                    }
                    case EcsMethodParameterProcessor.Result.EntityId:
                    {
                        _parameters.Add(new EntityIdEcsMethodParameter());
                        break;
                    }
                    case EcsMethodParameterProcessor.Result.Component:
                    {
                        var poolName = GetOrCreatePoolName(poolNames, parameterTypeName);
                        var hasIn = parameter.Modifiers.Any(SyntaxKind.InKeyword);
                        _parameters.Add(new ComponentEcsMethodParameter(poolName, parameterTypeName, hasIn)
                        );
                        break;
                    }
                    case EcsMethodParameterProcessor.Result.UnityEngineObject:
                    {
                        var fqUnityRefName = ConstructUnityRefName(parameterTypeName);
                        var poolName = GetOrCreatePoolName(poolNames, fqUnityRefName);
                        _parameters.Add(new UnityEngineObjectEcsMethodParameter(poolName, fqUnityRefName));
                        break;
                    }
                    case EcsMethodParameterProcessor.Result.EntityIdWithModifiers:
                    case EcsMethodParameterProcessor.Result.ComponentOfReferenceType:
                    case EcsMethodParameterProcessor.Result.ComponentDuplicate:
                    case EcsMethodParameterProcessor.Result.ComponentWithOutModifier:
                    case EcsMethodParameterProcessor.Result.ComponentWithoutInOrRef:
                    case EcsMethodParameterProcessor.Result.UnityEngineObjectRef:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            foreach (var parameter in _parameters)
            {
                var attribute = parameter.GetSystemComponentAccessAttributeOrDefault();
                if (attribute != null)
                    attributes.Add(attribute);
            }
        }

        private static string GetOrCreatePoolName(Dictionary<string, string> poolNames, string typeName)
        {
            if (!poolNames.TryGetValue(typeName, out var poolName))
            {
                poolName = $"_pool{poolNames.Count:00}";
                poolNames[typeName] = poolName;
            }

            return poolName;
        }

        private void CheckForConfigureFilterMethod()
        {
            var validator = new ConfigureFilterValidator();

            foreach (var member in _class.GetMembers())
            {
                if (!(member is IMethodSymbol methodSymbol)) continue;

                var result = validator.Run(methodSymbol);
                if (result == ConfigureFilterValidator.Result.InvalidName) continue;
                if (member.Name != ConfigureFilterMethodName) continue;
                if (result == ConfigureFilterValidator.Result.Valid)
                    _callConfigureFilter = true;

                break;
            }
        }

        private void CheckInlining()
        {
            var mdsHasPartialKeyword = _mds.Modifiers.Any(SyntaxKind.PartialKeyword);
            _addInlineAttribute = mdsHasPartialKeyword &&
                                  !_mds.AttributeLists.HasAttribute("MethodImpl") &&
                                  !_mds.AttributeLists.HasAttribute("MethodImplAttribute");
        }

        public void AddUsingStatements(SortedSet<string> usingStatements)
        {
            if (_addInlineAttribute)
                usingStatements.Add("using System.Runtime.CompilerServices;");
        }

        public abstract string GetInterfaceName();


        public void GenerateFilterInitialization(IndentedTextWriter indentWriter)
        {
            var filterExpression = "";

            const string worldExpression = "_world";
            foreach (var parameter in _parameters)
            {
                filterExpression = parameter.BuildFilter(filterExpression, worldExpression);
            }

            indentWriter.WriteLine("var {0}Mask = {1};", Prefix, filterExpression);

            if (_callConfigureFilter)
                indentWriter.WriteLine("{0}({1}Mask);", ConfigureFilterMethodName, Prefix);

            indentWriter.WriteLine("_{0}Filter = {0}Mask.End();", Prefix);
        }

        public void WriteFieldDefinitions(IndentedTextWriter indentWriter)
        {
            indentWriter.WriteLine("private EcsFilter _{0}Filter;", Prefix);
        }

        public void GenerateMethod(IndentedTextWriter indentWriter)
        {
            using (indentWriter.BeginScope($"public void {ImplementedMethodName}(EcsSystems systems)"))
            {
                using (indentWriter.BeginScope($"foreach (var __idx in _{Prefix}Filter)"))
                {
                    const string idExpression = "__idx";
                    if (_booleanReturnType)
                        indentWriter.Write("var __continue = ");
                    indentWriter.Write(_mds.Identifier.ToString());
                    indentWriter.Write("(");

                    var first = true;

                    foreach (var parameter in _parameters)
                    {
                        var argument = parameter.GetArgument(idExpression);
                        if (!first)
                            indentWriter.Write(", ");
                        indentWriter.WriteLine(argument);
                        first = false;
                    }

                    indentWriter.WriteLine(");");
                    if (_booleanReturnType)
                    {
                        indentWriter.WriteLine();
                        using (indentWriter.BeginScope("if (!__continue)"))
                        {
                            indentWriter.WriteLine("break;");
                        }
                    }
                }
            }

            if (!_addInlineAttribute) return;

            var methodSignature = _mds;
            if (methodSignature.Body != null)
                methodSignature =
                    methodSignature.RemoveNode(methodSignature.Body, SyntaxRemoveOptions.KeepNoTrivia)!;

            foreach (var attributeList in methodSignature.AttributeLists)
            {
                methodSignature =
                    methodSignature.RemoveNode(attributeList, SyntaxRemoveOptions.KeepNoTrivia)!;
            }

            if (methodSignature.SemicolonToken == default)
                methodSignature =
                    methodSignature.WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            indentWriter.WriteLine("[MethodImpl(MethodImplOptions.AggressiveInlining)]");
            indentWriter.WriteLine(methodSignature);
        }
    }
}