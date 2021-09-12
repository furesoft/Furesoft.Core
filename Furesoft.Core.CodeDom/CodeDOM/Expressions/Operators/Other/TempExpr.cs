using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other
{
    /// <summary>
    /// Used temporarily during the parsing process for special situations - specifically,
    /// as a temporary holder of compiler directives that need to be returned to a higher level.
    /// </summary>
    public class TempExpr : Expression
    {
        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        { }

        #endregion
    }
}
