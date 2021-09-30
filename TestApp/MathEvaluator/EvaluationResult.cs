using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using System.Collections.Generic;

namespace TestApp
{
    public class EvaluationResult
    {
        public List<Message> Errors { get; set; }
        public List<double> Values { get; set; } = new();
    }
}