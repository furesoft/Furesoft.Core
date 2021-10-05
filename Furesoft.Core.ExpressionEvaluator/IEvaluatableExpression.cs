namespace Furesoft.Core.ExpressionEvaluator
{
    public interface IEvaluatableExpression
    {
        double Evaluate(ExpressionParser ep, Scope scope);
    }
}