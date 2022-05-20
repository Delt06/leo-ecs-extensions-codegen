namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters
{
    public class ComponentEcsMethodParameter : IEcsMethodParameter
    {
        private readonly bool _isIn;
        private readonly string _poolName;
        private readonly string _typeName;

        public ComponentEcsMethodParameter(string poolName, string typeName, bool isIn)
        {
            _poolName = poolName;
            _typeName = typeName;
            _isIn = isIn;
        }

        public string GetArgument(string idExpression)
        {
            var getCall = $"{_poolName}.Get({idExpression})";
            return _isIn ? getCall : "ref " + getCall;
        }

        public string BuildFilter(string filterExpression, string worldExpression)
        {
            if (string.IsNullOrEmpty(filterExpression))
                return worldExpression + ".Filter<" + _typeName + ">()";
            return filterExpression + ".Inc<" + _typeName + ">()";
        }
    }
}