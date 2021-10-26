namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    public class ReflectionMacro : Macro
    {
        public readonly Func<MacroContext, Expression[], Expression> Callback;
        private readonly string _name;

        public ReflectionMacro(string name, Func<MacroContext, Expression[], Expression> callback)
        {
            this._name = name;
            this.Callback = callback;
        }

        public override string Name => _name;

        public override Expression Invoke(MacroContext context, params Expression[] arguments)
        {
            return Callback.Invoke(context, arguments);
        }
    }
}