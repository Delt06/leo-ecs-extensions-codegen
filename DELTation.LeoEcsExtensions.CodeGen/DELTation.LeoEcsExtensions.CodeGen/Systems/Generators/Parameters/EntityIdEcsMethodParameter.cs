namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters
{
	public class EntityIdEcsMethodParameter : IEcsMethodParameter
	{
		public string GetArgument(string idExpression) => idExpression;
		public string BuildFilter(string filterExpression, string worldExpression) => filterExpression;
	}
}