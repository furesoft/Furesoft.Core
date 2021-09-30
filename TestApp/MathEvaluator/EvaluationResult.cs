using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using System.Collections.Generic;
using TestApp;

namespace TestApp.MathEvaluator
{
    public class EvaluationResult
    {
        public List<Message> Errors { get; set; }
        public List<double> Values { get; set; } = new();
    }
}
