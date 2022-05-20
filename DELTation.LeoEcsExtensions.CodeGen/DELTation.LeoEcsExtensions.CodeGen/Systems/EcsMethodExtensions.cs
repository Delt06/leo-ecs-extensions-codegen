using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems
{
    public static class EcsMethodExtensions
    {
        private const string EcsRun = "EcsRun";
        private const string EcsRunAttribute = EcsRun + "Attribute";
        private const string EcsInit = "EcsInit";
        private const string EcsInitAttribute = EcsInit + "Attribute";
        private const string EcsDestroy = "EcsDestroy";
        private const string EcsDestroyAttribute = EcsDestroy + "Attribute";

        public static bool HasAnyEcsMethodAttribute(this SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.HasEcsRunAttribute() ||
            attributeLists.HasEcsInitAttribute() ||
            attributeLists.HasEcsDestroyAttribute();

        public static bool HasEcsRunAttribute(this SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.HasAttribute(EcsRun) || attributeLists.HasAttribute(EcsRunAttribute);

        public static bool HasEcsInitAttribute(this SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.HasAttribute(EcsInit) || attributeLists.HasAttribute(EcsInitAttribute);

        public static bool HasEcsDestroyAttribute(this SyntaxList<AttributeListSyntax> attributeLists) =>
            attributeLists.HasAttribute(EcsDestroy) || attributeLists.HasAttribute(EcsDestroyAttribute);

        public static bool IsAnyEcsMethodAttribute(this AttributeSyntax attributeSyntax) =>
            attributeSyntax.IsEcsRunAttribute() ||
            attributeSyntax.IsEcsInitAttribute() ||
            attributeSyntax.IsEcsDestroyAttribute();


        public static T SwitchOnEcsMethodAttribute<T>(this AttributeSyntax attributeSyntax, Func<T> onRun,
            Func<T> onInit, Func<T> onDestroy, Func<T> fallback)
        {
            if (attributeSyntax.IsEcsRunAttribute())
                return onRun();
            if (attributeSyntax.IsEcsInitAttribute())
                return onInit();
            if (attributeSyntax.IsEcsDestroyAttribute())
                return onDestroy();
            return fallback();
        }

        public static T SwitchOnEcsMethodAttribute<T>(this MethodDeclarationSyntax mds, Func<T> onRun,
            Func<T> onInit, Func<T> onDestroy, Func<T> fallback)
        {
            var attributeLists = mds.AttributeLists;
            if (attributeLists.HasEcsRunAttribute())
                return onRun();
            if (attributeLists.HasEcsInitAttribute())
                return onInit();
            if (attributeLists.HasEcsDestroyAttribute())
                return onDestroy();
            return fallback();
        }

        public static bool IsEcsRunAttribute(this AttributeSyntax attributeSyntax)
        {
            var name = attributeSyntax.Name.ToString();
            return name == EcsRun || name == EcsRunAttribute;
        }

        public static bool IsEcsInitAttribute(this AttributeSyntax attributeSyntax)
        {
            var name = attributeSyntax.Name.ToString();
            return name == EcsInit || name == EcsInitAttribute;
        }

        public static bool IsEcsDestroyAttribute(this AttributeSyntax attributeSyntax)
        {
            var name = attributeSyntax.Name.ToString();
            return name == EcsDestroy || name == EcsDestroyAttribute;
        }


        public static IEnumerable<MethodDeclarationSyntax>
            GetMethodsWithAnyEcsMethodAttribute(this ClassDeclarationSyntax @class) =>
            @class.Members
                .OfType<MethodDeclarationSyntax>()
                .Where(mds => mds.AttributeLists.HasAnyEcsMethodAttribute());
    }
}