using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Methods;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators
{
    [Generator]
    public class SystemSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new EcsMethodSyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is EcsMethodSyntaxReceiver mySyntaxReceiver)) return;

            foreach (var kvp in mySyntaxReceiver.Candidates)
            {
                var cds = kvp.Key;
                var methods = kvp.Value;

                if (!cds.Modifiers.Any(SyntaxKind.PartialKeyword))
                    continue;

                var semanticModel = context.Compilation.GetSemanticModel(cds.SyntaxTree);
                var poolNames = new Dictionary<string, string>();

                var @class = semanticModel.GetType(cds);
                var methodGenerators =
                    CreateAndInitMethodsGenerators(context, methods, @class, poolNames, semanticModel);

                using var writer = new StringWriter();
                using var indentWriter = new IndentedTextWriter(writer);

                var usingStatements = new SortedSet<string>
                {
                    "using Leopotam.EcsLite;",
                };

                foreach (var methodGenerator in methodGenerators)
                {
                    methodGenerator.AddUsingStatements(usingStatements);
                }

                var compilationUnitSyntax = cds.SyntaxTree.GetCompilationUnitRoot();
                foreach (var usingDirectiveSyntax in compilationUnitSyntax.Usings)
                {
                    usingStatements.Add(usingDirectiveSyntax.ToString());
                }

                foreach (var usingStatement in usingStatements)
                {
                    indentWriter.WriteLine(usingStatement);
                }

                if (usingStatements.Count > 0)
                    indentWriter.WriteLine();

                string? @namespace = null;

                if (SyntaxNodeHelper.TryGetParentSyntax(cds, out NamespaceDeclarationSyntax nds))
                    @namespace = nds.Name.ToString();


                if (@namespace != null)
                {
                    indentWriter.WriteLine("namespace {0}", @namespace);
                    indentWriter.OpenBrace();
                }

                var className = cds.Identifier.ToString();

                var interfaces = methodGenerators
                    .Select(mg => mg.GetInterfaceName())
                    .Append("IEcsPreInitSystem")
                    .Distinct()
                    .OrderBy(i => i);
                var interfacesListString = string.Join(", ", interfaces);

                using (indentWriter.BeginScope($"public partial class {className} : {interfacesListString}"))
                {
                    const string worldExpression = "_world";
                    indentWriter.WriteLine("private EcsWorld {0};", worldExpression);

                    foreach (var methodGenerator in methodGenerators)
                    {
                        methodGenerator.WriteFieldDefinitions(indentWriter);
                    }

                    foreach (var poolNameKvp in poolNames)
                    {
                        indentWriter.WriteLine("private EcsPool<{0}> {1};", poolNameKvp.Key, poolNameKvp.Value);
                    }

                    using (indentWriter.BeginScope("public void PreInit(EcsSystems systems)"))
                    {
                        indentWriter.WriteLine("{0} = systems.GetWorld();", worldExpression);

                        foreach (var methodGenerator in methodGenerators)
                        {
                            methodGenerator.GenerateFilterInitialization(indentWriter);
                        }

                        foreach (var poolNameKvp in poolNames)
                        {
                            indentWriter.WriteLine("{0} = {1}.GetPool<{2}>();",
                                poolNameKvp.Value, worldExpression, poolNameKvp.Key
                            );
                        }
                    }

                    foreach (var methodGenerator in methodGenerators)
                    {
                        methodGenerator.GenerateMethod(indentWriter);
                    }
                }

                if (@namespace != null)
                    indentWriter.CloseBrace();

                context.AddSource($"{className}.g.cs", writer.ToString());
            }
        }

        private static List<EcsMethodGeneratorBase> CreateAndInitMethodsGenerators(GeneratorExecutionContext context,
            List<MethodDeclarationSyntax> methods,
            INamedTypeSymbol @class, Dictionary<string, string> poolNames, SemanticModel semanticModel)
        {
            var methodGenerators = new List<EcsMethodGeneratorBase>();

            foreach (var mds in methods)
            {
                var generator = mds.SwitchOnEcsMethodAttribute<EcsMethodGeneratorBase>(
                    () => new EcsRunGenerator(mds, @class),
                    () => new EcsInitGenerator(mds, @class),
                    () => new EcsDestroyGenerator(mds, @class),
                    () => throw new InvalidOperationException("Method does not have any ECS method attribute")
                );
                generator.Init(poolNames, semanticModel, context.Compilation);
                methodGenerators.Add(generator);
            }

            return methodGenerators;
        }

        private class EcsMethodSyntaxReceiver : ISyntaxReceiver
        {
            public Dictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>> Candidates { get; } =
                new Dictionary<ClassDeclarationSyntax, List<MethodDeclarationSyntax>>();

            public void OnVisitSyntaxNode(SyntaxNode node)
            {
                if (!(node is MethodDeclarationSyntax { Parent: ClassDeclarationSyntax cds } mds)) return;
                if (!mds.AttributeLists.HasAnyEcsMethodAttribute()) return;

                if (!Candidates.TryGetValue(cds, out var mdsList))
                    Candidates[cds] = mdsList = new List<MethodDeclarationSyntax>();

                mdsList.Add(mds);
            }
        }
    }
}