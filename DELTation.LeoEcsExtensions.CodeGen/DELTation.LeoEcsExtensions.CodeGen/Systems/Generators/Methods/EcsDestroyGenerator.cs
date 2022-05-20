using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Methods
{
	public class EcsDestroyGenerator : EcsMethodGeneratorBase
	{
		public EcsDestroyGenerator(MethodDeclarationSyntax mds, INamedTypeSymbol @class) : base(mds, @class) { }

		protected override string Prefix => "destroy";

		protected override string ConfigureFilterMethodName => Constants.ConfigureDestroyFilter;

		protected override string ImplementedMethodName => "Destroy";

		public override string GetInterfaceName() => "IEcsDestroySystem";
	}
}