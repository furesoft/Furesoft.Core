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