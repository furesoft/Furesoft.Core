using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using System;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    public class ReflectionMacro : Macro
    {
        private readonly Func<MacroContext, Expression[], Expression> _callback;
        private readonly string _name;

        public ReflectionMacro(string name, Func<MacroContext, Expression[], Expression> callback)
        {
            this._name = name;
            this._callback = callback;
        }

        public override string Name => _name;

        public override Expression Invoke(MacroContext context, params Expression[] arguments)
        {
            return _callback.Invoke(context, arguments);
        }
    }
}