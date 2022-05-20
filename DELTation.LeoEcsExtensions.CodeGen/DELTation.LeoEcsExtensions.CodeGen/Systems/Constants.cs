namespace DELTation.LeoEcsExtensions.CodeGen.Systems
{
    public static class Constants
    {
        public const string DiagnosticCategory = "LEOECSEXTENSIONS";
        public const string ConfigureRunFilter = nameof(ConfigureRunFilter);
        public const string ConfigureInitFilter = nameof(ConfigureInitFilter);
        public const string ConfigureDestroyFilter = nameof(ConfigureDestroyFilter);
        public const string VoidFullyQualifiedName = "System.Void";
        public const string BoolFullyQualifiedName = "System.Boolean";
        public const string SystemComponentAccess = nameof(SystemComponentAccess);
        public const string IgnoreInferredSystemComponentAccess = nameof(IgnoreInferredSystemComponentAccess);
        public const string ComponentAccessTypeUnstructured = "ComponentAccessType.Unstructured";
        public const string ComponentAccessTypeReadOnly = "ComponentAccessType.ReadOnly";
        public const string ComponentAccessTypeReadWrite = "ComponentAccessType.ReadWrite";
    }
}