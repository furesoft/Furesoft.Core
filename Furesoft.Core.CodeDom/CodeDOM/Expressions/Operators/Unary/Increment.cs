using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary
{
    /// <summary>
    /// Increments an <see cref="Expression"/>, which should evaluate to a <see cref="VariableRef"/> (or a property or indexer access).
    /// </summary>
    public class Increment : PreUnaryOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Increment";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "++";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        /// <summary>
        /// Create an <see cref="Increment"/> operator.
        /// </summary>
        public Increment(Expression expression)
            : base(expression)
        { }

        protected Increment(Parser parser, CodeObject parent)
                    : base(parser, parent, false)
        { }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse an <see cref="Increment"/> operator.
        /// </summary>
        public static Increment Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // If we have an unused left expression, abort
            // (this is to give the PostIncrement operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return null;
            return new Increment(parser, parent);
        }

        /// <summary>
        /// The internal name of the <see cref="UnaryOperator"/>.
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
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, true, Parse);
        }
    }
}
