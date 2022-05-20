using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace DELTation.LeoEcsExtensions.CodeGen
{
	public static class GenerationExtensions
	{
		public static StructDeclarationSyntax AddAttributes(this StructDeclarationSyntax sds,
			params string[] attributeName) =>
			sds.AddAttributeLists(
				attributeName
					.Select(n => AttributeList(SingletonSeparatedList(Attribute(ParseName(n)))))
					.ToArray()
			);

		public static string GetIdentifierWithTypeParameters(this StructDeclarationSyntax sds)
		{
			var identifier = sds.Identifier.ToString();
			if (sds.TypeParameterList == null)
				return identifier;

			var commaSeparatedTypeParameters =
				string.Join(", ", sds.TypeParameterList.Parameters
					.Select(p => p.Identifier.ToString())
				);
			return $"{identifier}<{commaSeparatedTypeParameters}>";
		}

		public static SyntaxNode TryAddUsingStatement(this SyntaxNode root, string @using)
		{
			if (root is CompilationUnitSyntax compilationUnitSyntax)
			{
				var usingDirectiveSyntax = UsingDirective(ParseName(@using));
				if (compilationUnitSyntax.Usings.All(u => u.Name.ToString() != @using))
					root = compilationUnitSyntax.AddUsings(usingDirectiveSyntax);
			}

			return root;
		}

		public static bool IsAccessModifierKeyword(this SyntaxToken token) => token.IsKind(SyntaxKind.PrivateKeyword)
		                                                                      ||
		                                                                      token.IsKind(SyntaxKind.PublicKeyword)
		                                                                      ||
		                                                                      token.IsKind(SyntaxKind.InternalKeyword)
		                                                                      ||
		                                                                      token.IsKind(SyntaxKind.ProtectedKeyword);

		public static bool HasAttribute(this SyntaxList<AttributeListSyntax> attributeLists, string attributeName)
		{
			foreach (var attributeList in attributeLists)
			{
				foreach (var attribute in attributeList.Attributes)
				{
					if (attribute.Name.ToString() == attributeName)
						return true;
				}
			}

			return false;
		}

		public static string GetFullyQualifiedName(this ISymbol? s)
		{
			var friendlyName = s.GetFullyQualifiedNameBase();
			if (s is INamedTypeSymbol { IsGenericType: true } nts)
			{
				var iBacktick = friendlyName.IndexOf('`');
				if (iBacktick > 0) friendlyName = friendlyName.Remove(iBacktick);
				friendlyName += "<";
				var typeParameters = nts.TypeArguments;
				for (var i = 0; i < typeParameters.Length; ++i)
				{
					var typeParamName = typeParameters[i].GetFullyQualifiedName();
					friendlyName += i == 0 ? typeParamName : "," + typeParamName;
				}

				friendlyName += ">";
			}

			return friendlyName;
		}

		private static string GetFullyQualifiedNameBase(this ISymbol? s)
		{
			if (s == null || IsRootNamespace(s)) return string.Empty;
			var sb = new StringBuilder(s.MetadataName);
			var last = s;

			s = s.ContainingSymbol;

			while (!IsRootNamespace(s))
			{
				if (s is ITypeSymbol && last is ITypeSymbol)
					sb.Insert(0, '+');
				else
					sb.Insert(0, '.');

				sb.Insert(0, s.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
				s = s.ContainingSymbol;
			}

			return sb.ToString();
		}

		private static bool IsRootNamespace(ISymbol? symbol)
		{
			INamespaceSymbol? s;
			return (s = symbol as INamespaceSymbol) != null && s.IsGlobalNamespace;
		}

		public static INamedTypeSymbol GetType(this SemanticModel semanticModel, ParameterSyntax parameter) =>
			semanticModel.GetType(parameter.Type!);

		public static INamedTypeSymbol GetType(this SemanticModel semanticModel, TypeSyntax type) =>
			(INamedTypeSymbol)semanticModel.GetSymbolInfo(type).Symbol!;

		public static IMethodSymbol GetMethod(this SemanticModel semanticModel, MethodDeclarationSyntax mds) =>
			semanticModel.GetDeclaredSymbol(mds)!;

		public static INamedTypeSymbol GetType(this SemanticModel semanticModel, ClassDeclarationSyntax @class) =>
			semanticModel.GetDeclaredSymbol(@class)!;
	}
}