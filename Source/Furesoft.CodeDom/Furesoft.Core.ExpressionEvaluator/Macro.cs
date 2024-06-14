namespace Furesoft.Core.ExpressionEvaluator;

public delegate Expression MacroInvokeDelegate(MacroContext context, params Expression[] arguments);

public abstract class Macro
{
    public List<ChildList<Expression>> Rules = [];

    public abstract string Name { get; }

    public abstract Expression Invoke(MacroContext context, params Expression[] arguments);

    public static Channel OutputChannel { get; } = new();
}
