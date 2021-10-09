using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using System.Linq;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    public class RuleForMacro : Macro
    {
        public override string Name => "rulefor";

        public override Expression Invoke(MacroContext context, params Expression[] arguments)
        {
            if (arguments[0] is UnresolvedRef nameRef && nameRef.Reference is string name)
            {
                context.AddRule(name, new(arguments.Skip(1)));
            }

            return new TempExpr();
        }
    }
}