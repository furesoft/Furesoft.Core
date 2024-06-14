
using Furesoft.Core.CodeDom.CodeDOM.Annotations;

namespace Furesoft.Core.ExpressionEvaluator;

public class EvaluationResult
{
    public List<Message> Errors { get; set; } = [];
    public string ModuleName { get; internal set; }
    public List<ValueType> Values { get; set; } = [];
}
