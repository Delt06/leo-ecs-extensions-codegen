using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Components
{
    public static class EcsComponentsExtensions
    {
        public const string EcsComponentWithView = nameof(EcsComponentWithView);
        private const string EcsComponentWithViewAttribute = EcsComponentWithView + "Attribute";
        private const string EcsComponent = nameof(EcsComponent);
        private const string EcsComponentAttribute = EcsComponent + "Attribute";
        public const string Serializable = nameof(Serializable);

        public static bool HasEcsComponentWithViewAttribute(this SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.HasAttribute(EcsComponentWithView) ||
            attributeLists.HasAttribute(EcsComponentWithViewAttribute);

        public static bool HasEcsComponentAttribute(this SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.HasAttribute(EcsComponent) || attributeLists.HasAttribute(EcsComponentAttribute);
    }
}