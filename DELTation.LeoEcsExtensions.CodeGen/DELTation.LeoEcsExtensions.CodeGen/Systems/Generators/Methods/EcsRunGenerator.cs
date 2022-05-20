using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Methods
{
	public class EcsRunGenerator : EcsMethodGeneratorBase
	{
		public EcsRunGenerator(MethodDeclarationSyntax mds, INamedTypeSymbol @class) : base(mds, @class) { }

		protected override string Prefix => "run";

		protected override string ConfigureFilterMethodName => Constants.ConfigureRunFilter;

		protected override string ImplementedMethodName => "Run";

		public override string GetInterfaceName() => "IEcsRunSystem";
	}
}