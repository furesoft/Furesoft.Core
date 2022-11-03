using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;

/// <summary>
/// Used temporarily during the parsing process for special situations - specifically,
/// as a temporary holder of compiler directives that need to be returned to a higher level.
/// </summary>
public class TempExpr : Expression
{
    public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
    { }
}
