using System;
using ValueType = Maki.Variant<double, MathNet.Numerics.LinearAlgebra.Matrix<double>>;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class OperatorOverload
    {
        public Func<object, object, ValueType> Invoker { get; set; }

        public Type Left { get; set; }
        public Type Right { get; set; }

        public string Symbol { get; set; }
    }
}