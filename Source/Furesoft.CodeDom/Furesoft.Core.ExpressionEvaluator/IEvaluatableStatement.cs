namespace Furesoft.Core.ExpressionEvaluator;

public interface IEvaluatableStatement
{
    void Evaluate(ExpressionParser ep);
}
