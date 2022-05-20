using Microsoft.CodeAnalysis;

namespace DELTation.LeoEcsExtensions.CodeGen
{
	internal static class SyntaxNodeHelper
	{
		public static bool TryGetParentSyntax<T>(SyntaxNode? syntaxNode, out T result)
			where T : SyntaxNode
		{
			// set defaults
			result = null!;

			if (syntaxNode == null) return false;

			try
			{
				syntaxNode = syntaxNode.Parent;

				if (syntaxNode == null) return false;

				if (syntaxNode is T castedSyntaxNode)
				{
					result = castedSyntaxNode;
					return true;
				}

				return TryGetParentSyntax(syntaxNode, out result);
			}
			catch
			{
				return false;
			}
		}
	}
}