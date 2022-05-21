using static DELTation.LeoEcsExtensions.CodeGen.Systems.Constants;

namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters
{
    public class UnityEngineObjectEcsMethodParameter : IEcsMethodParameter
    {
        private readonly string _fqUnityRefName;
        private readonly string _poolName;

        public UnityEngineObjectEcsMethodParameter(string poolName, string fqUnityRefName)
        {
            _poolName = poolName;
            _fqUnityRefName = fqUnityRefName;
        }

        public string GetArgument(string idExpression) => $"{_poolName}.Get({idExpression}).Object";

        public string BuildFilter(string filterExpression, string worldExpression)
        {
            if (string.IsNullOrEmpty(filterExpression))
                return worldExpression + ".Filter<" + _fqUnityRefName + ">()";
            return filterExpression + ".Inc<" + _fqUnityRefName + ">()";
        }

        public string GetSystemComponentAccessAttributeOrDefault() =>
            $"[{SystemComponentAccess}(typeof({_fqUnityRefName}), {ComponentAccessTypeReadOnly})]";
    }
}