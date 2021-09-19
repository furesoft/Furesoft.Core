using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base
{
    /// <summary>
    /// The common base class of all prefix unary operators (<see cref="Cast"/>, <see cref="Complement"/>,
    /// <see cref="Decrement"/>, <see cref="Increment"/>, <see cref="Negative"/>, <see cref="Not"/>, <see cref="Positive"/>).
    /// </summary>
    public abstract class PreUnaryOperator : UnaryOperator
    {
        protected PreUnaryOperator(Expression expression)
            : base(expression)
        { }

        protected PreUnaryOperator(Parser parser, CodeObject parent, bool skipParsing)
            : base(parser, parent)
        {
            if (!skipParsing)
            {
                parser.NextToken();  // Skip past the operator
                SetField(ref _expression, Parse(parser, this), false);

                // Move any EOL or Postfix annotations from the expression up to the parent if there are no
                // parens in the way - this "normalizes" the annotations to the highest node on the line.
                if (_expression != null && _expression.HasEOLOrPostAnnotations && parent != parser.GetNormalizationBlocker())
                    MoveEOLAndPostAnnotations(_expression);
            }
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
            if (_expression != null)
                _expression.AsText(writer, flags);
        }
    }
}
