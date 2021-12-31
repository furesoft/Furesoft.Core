namespace Furesoft.Core.ExpressionEvaluator;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ParameterDescriptionAttribute : Attribute
{
    public ParameterDescriptionAttribute(string parameterName, string description)
    {
        ParameterName = parameterName;
        Description = description;
    }

    public string Description { get; set; }
    public string ParameterName { get; set; }
}
