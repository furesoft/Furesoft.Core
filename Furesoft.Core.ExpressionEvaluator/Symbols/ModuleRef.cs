using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;

namespace Furesoft.Core.ExpressionEvaluator.Symbols
{
    public class ModuleRef : SymbolicRef
    {
        public ModuleRef(Module module) : base(module)
        {
        }

        public ModuleRef(Scope module) : base(module)
        {
        }
    }
}