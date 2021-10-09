using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    public class ResolveMacro : Macro
    {
        public override string Name => "resolve";

        public override Expression Invoke(MacroContext context, params Expression[] arguments)
        {
            var r = Rules;

            throw new System.NotImplementedException();
        }
    }
}