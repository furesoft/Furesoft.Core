using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic
{
    /// <summary>
    /// Subtracts one <see cref="Expression"/> from another.
    /// </summary>
    public class Subtract : BinaryArithmeticOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Subtraction";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "-";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 310;

        /// <summary>
        /// Create a <see cref="Subtract"/> operator.
        /// </summary>
        public Subtract(Expression left, Expression right)
            : base(left, right)
        { }

        protected Subtract(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse a <see cref="Subtract"/> operator.
        /// </summary>
        public static Subtract Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Negative operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new Subtract(parser, parent);
            return null;
        }

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return InternalName;
        }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}
