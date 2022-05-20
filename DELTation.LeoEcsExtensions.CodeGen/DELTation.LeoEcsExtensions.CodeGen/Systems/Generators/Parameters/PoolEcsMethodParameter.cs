using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters
{
    public class PoolEcsMethodParameter : IEcsMethodParameter
    {
        private readonly string _poolName;
        private readonly string _typeName;

        public PoolEcsMethodParameter(string poolName, string typeName)
        {
            _poolName = poolName;
            _typeName = typeName;
        }

        public string GetArgument(string idExpression) => _poolName;

        public string BuildFilter(string filterExpression, string worldExpression) => filterExpression;

        public string GetSystemComponentAccessAttributeOrDefault() =>
            $"[{SystemComponentAccess}(typeof({_typeName}), {ComponentAccessTypeUnstructured})]";
    }
}