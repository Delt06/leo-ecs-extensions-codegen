using System;

// ReSharper disable once CheckNamespace
namespace DELTation.LeoEcsExtensions.Systems
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SystemComponentAccessAttribute : Attribute
    {
        public readonly ComponentAccessType AccessType;
        public readonly Type Type;

        public SystemComponentAccessAttribute(Type type, ComponentAccessType accessType)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            AccessType = accessType;
        }
    }

    public enum ComponentAccessType
    {
        Unstructured,
        ReadOnly,
        ReadWrite,
        Observable,
    }
}