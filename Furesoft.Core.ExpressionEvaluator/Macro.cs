using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using System.Collections.Generic;

namespace Furesoft.Core.ExpressionEvaluator
{
    public delegate Expression MacroInvokeDelegate(MacroContext context, params Expression[] arguments);

    public abstract class Macro
    {
        public List<ChildList<Expression>> Rules = new();

        public abstract string Name { get; }

        public abstract Expression Invoke(MacroContext context, params Expression[] arguments);
    }
}