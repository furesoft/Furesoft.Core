using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of <see cref="LeftShift"/> and <see cref="RightShift"/>.
    /// </summary>
    public abstract class BinaryShiftOperator : BinaryOperator
    {
        protected BinaryShiftOperator(Expression left, Expression right)
            : base(left, right)
        { }

        protected BinaryShiftOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}