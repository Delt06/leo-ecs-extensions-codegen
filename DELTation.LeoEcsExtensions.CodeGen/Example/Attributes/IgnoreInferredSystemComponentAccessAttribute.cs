using System;

// ReSharper disable once CheckNamespace
namespace DELTation.LeoEcsExtensions.Systems
{
    [AttributeUsage(AttributeTargets.Class)]
    public class IgnoreInferredSystemComponentAccessAttribute : Attribute { }
}