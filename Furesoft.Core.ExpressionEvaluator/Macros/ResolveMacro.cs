using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    public class ResolveMacro : Macro
    {
        public override string Name => "resolve";

        public override Expression Invoke(MacroContext context, params Expression[] arguments)
        {
            //ToDo: implement resolve macro

            if (arguments[0] is Assignment a && arguments[1] is UnresolvedRef variable)
            {
                context.ParentCallNode.AttachMessage("There is no rule defined for " + a.AsText(), MessageSeverity.Hint, MessageSource.Resolve);
            }

            return 6;
        }
    }
}