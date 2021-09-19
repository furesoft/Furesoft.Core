using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary bitwise operators (<see cref="BitwiseAnd"/>, <see cref="BitwiseOr"/>, <see cref="BitwiseXor"/>).
    /// </summary>
    public abstract class BinaryBitwiseOperator : BinaryOperator
    {
        protected BinaryBitwiseOperator(Expression left, Expression right)
            : base(left, right)
        { }

        protected BinaryBitwiseOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }  // Default to NO parens for binary bitwise operators
        }
    }
}