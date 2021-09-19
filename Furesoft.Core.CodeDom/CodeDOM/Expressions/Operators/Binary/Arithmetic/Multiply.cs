using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic
{
    /// <summary>
    /// Multiplies one <see cref="Expression"/> by another.
    /// </summary>
    public class Multiply : BinaryArithmeticOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Multiply";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "*";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 300;

        /// <summary>
        /// Create a <see cref="Multiply"/> operator.
        /// </summary>
        public Multiply(Expression left, Expression right)
            : base(left, right)
        { }

        protected Multiply(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="Multiply"/> operator.
        /// </summary>
        public static Multiply Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the PointerIndirection operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new Multiply(parser, parent);
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
            // Use a parse-priority of 100
            Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, false, Parse);
        }
    }
}
