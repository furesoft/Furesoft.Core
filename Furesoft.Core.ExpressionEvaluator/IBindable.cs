using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Furesoft.Core.ExpressionEvaluator
{
    public interface IBindable
    {
        CodeObject Bind(ExpressionParser ep);
    }
}