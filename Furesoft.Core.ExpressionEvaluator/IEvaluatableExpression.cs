namespace Furesoft.Core.ExpressionEvaluator
{
    using ValueType = Maki.Variant<double, MathNet.Numerics.LinearAlgebra.Matrix<double>>;

    public interface IEvaluatableExpression
    {
        ValueType Evaluate(ExpressionParser ep, Scope scope);
    }
}