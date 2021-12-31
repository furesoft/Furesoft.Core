namespace Furesoft.Core.ExpressionEvaluator;

[AttributeUsage(AttributeTargets.Method)]
public class FunctionNameAttribute : Attribute
{
    public FunctionNameAttribute(string name)
    {
        Name = name;
    }

    public string Name { get; set; }
}
