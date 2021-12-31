using System.ComponentModel;

namespace Furesoft.Core.ExpressionEvaluator.Macros;

[Description("Get The derivative function")]
[ParameterDescription("body", "The body to derive")]
public class DeriveMacro : Macro
{
    public override string Name => "derive";

    public override Expression Invoke(MacroContext context, params Expression[] arguments)
    {
        return new TempExpr();
    }
}