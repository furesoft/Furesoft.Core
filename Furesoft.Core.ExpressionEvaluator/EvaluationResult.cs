using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using System.Collections.Generic;

using ValueType = Maki.Variant<double, MathNet.Numerics.LinearAlgebra.Matrix<double>>;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class EvaluationResult
    {
        public List<Message> Errors { get; set; } = new();
        public string ModuleName { get; internal set; }
        public List<ValueType> Values { get; set; } = new();
    }
}