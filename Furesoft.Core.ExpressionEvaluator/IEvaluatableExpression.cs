namespace Furesoft.Core.ExpressionEvaluator;

using ValueType = ValueType;

public interface IEvaluatableExpression
{
    ValueType Evaluate(ExpressionParser ep, Scope scope);
}
