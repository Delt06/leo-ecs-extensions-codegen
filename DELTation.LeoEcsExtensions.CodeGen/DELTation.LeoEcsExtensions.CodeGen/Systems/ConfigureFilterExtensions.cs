using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems
{
	public static class ConfigureFilterExtensions
	{
		public static string? GetConfigureFilterMethodNameOrDefault(this AttributeSyntax attributeSyntax) =>
			attributeSyntax.SwitchOnEcsMethodAttribute<string?>(
				() => ConfigureRunFilter,
				() => ConfigureInitFilter,
				() => ConfigureDestroyFilter,
				() => null
			);

		public static bool IsAnyOfConfigureFilterMethods(this IMethodSymbol methodSymbol)
		{
			var methodName = methodSymbol.Name;
			return methodName == ConfigureRunFilter ||
			       methodName == ConfigureInitFilter ||
			       methodName == ConfigureDestroyFilter;
		}
	}
}