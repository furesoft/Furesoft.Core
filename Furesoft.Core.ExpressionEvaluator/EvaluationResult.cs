using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using System.Collections.Generic;

namespace Furesoft.Core.ExpressionEvaluator
{
    public class EvaluationResult
    {
        public List<Message> Errors { get; set; }
        public List<double> Values { get; set; } = new();
        public string ModuleName { get; internal set; }
    }
}
