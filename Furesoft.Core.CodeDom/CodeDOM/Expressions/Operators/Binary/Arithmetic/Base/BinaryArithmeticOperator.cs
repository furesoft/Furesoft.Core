using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base
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
