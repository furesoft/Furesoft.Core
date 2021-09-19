using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary arithmetic operators (<see cref="Add"/>, <see cref="Subtract"/>, <see cref="Multiply"/>, <see cref="Divide"/>, <see cref="Mod"/>).
    /// </summary>
    public abstract class BinaryArithmeticOperator : BinaryOperator
    {
        protected BinaryArithmeticOperator(Expression left, Expression right)
            : base(left, right)
        { }

        protected BinaryArithmeticOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }  // Default to NO parens for binary arithmetic operators
        }
    }
}