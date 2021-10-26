using System.ComponentModel;

namespace Furesoft.Core.ExpressionEvaluator.Macros
{
    [Description("Define a rule for a macro to change its behavior or add features")]
    [ParameterDescriptionAttribute("macro", "The macro to affect")]
    [ParameterDescriptionAttribute("rule", "The rule definitions")]
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