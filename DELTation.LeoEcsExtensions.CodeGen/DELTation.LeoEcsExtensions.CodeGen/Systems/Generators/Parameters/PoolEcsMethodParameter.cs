namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters
{
	public class PoolEcsMethodParameter : IEcsMethodParameter
	{
		private readonly string _poolName;

		public PoolEcsMethodParameter(string poolName) => _poolName = poolName;

		public string GetArgument(string idExpression) => _poolName;

		public string BuildFilter(string filterExpression, string worldExpression) => filterExpression;
	}
}