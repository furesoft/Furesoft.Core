using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all postfix unary operators (<see cref="PostIncrement"/> and <see cref="PostDecrement"/>).
    /// </summary>
    public abstract class PostUnaryOperator : UnaryOperator
    {
        protected PostUnaryOperator(Expression expression)
            : base(expression)
        { }

        protected PostUnaryOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Expression expression = parser.RemoveLastUnusedExpression();
            MoveFormatting(expression);
            SetField(ref _expression, expression, false);

            parser.NextToken();  // Skip past the operator
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            _expression.AsText(writer, flags);
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
        }
    }
}