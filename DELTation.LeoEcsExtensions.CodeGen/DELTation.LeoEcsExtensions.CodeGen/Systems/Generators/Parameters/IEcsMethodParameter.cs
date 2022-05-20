namespace DELTation.LeoEcsExtensions.CodeGen.Systems.Generators.Parameters
{
    public interface IEcsMethodParameter
    {
        string GetArgument(string idExpression);
        string BuildFilter(string filterExpression, string worldExpression);
        string? GetSystemComponentAccessAttributeOrDefault();
    }
}