using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;

namespace TestApp.MathEvaluator
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