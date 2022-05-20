using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Methods
{
    public class EcsInitGenerator : EcsMethodGeneratorBase
    {
        public EcsInitGenerator(MethodDeclarationSyntax mds, INamedTypeSymbol @class) : base(mds, @class) { }

        protected override string Prefix => "init";

        protected override string ConfigureFilterMethodName => Constants.ConfigureInitFilter;

        protected override string ImplementedMethodName => "Init";

        public override string GetInterfaceName() => "IEcsInitSystem";
    }
}