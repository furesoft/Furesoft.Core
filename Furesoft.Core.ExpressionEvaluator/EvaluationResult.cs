using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Maki;
using System.Collections.Generic;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class EvaluationResult
    {
        public List<Message> Errors { get; set; } = new();
        public string ModuleName { get; internal set; }
        public List<Variant<double>> Values { get; set; } = new();
    }
}