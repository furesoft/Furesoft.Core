using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base
{
    /// <summary>
    /// The common base class of all postfix unary operators (<see cref="PostIncrement"/> and <see cref="PostDecrement"/>).
    /// </summary>
    public abstract class PostUnaryOperator : UnaryOperator
    {
        #region /* CONSTRUCTORS */

        protected PostUnaryOperator(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        protected PostUnaryOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Expression expression = parser.RemoveLastUnusedExpression();
            MoveFormatting(expression);
            SetField(ref _expression, expression, false);

            parser.NextToken();  // Skip past the operator
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            _expression.AsText(writer, flags);
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
        }

        #endregion
    }
}
