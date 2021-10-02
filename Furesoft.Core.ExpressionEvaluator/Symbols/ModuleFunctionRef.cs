using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;

namespace Furesoft.Core.ExpressionEvaluator.Symbols
{
    public class ModuleFunctionRef : SymbolicRef
    {
        public ModuleFunctionRef(Module module, Call call) : base(module)
        {
            Call = call;
        }

        public Call Call { get; }
    }
}