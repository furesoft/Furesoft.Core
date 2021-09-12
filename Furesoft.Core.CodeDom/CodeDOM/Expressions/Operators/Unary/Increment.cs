using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary
{
    /// <summary>
    /// Increments an <see cref="Expression"/>, which should evaluate to a <see cref="VariableRef"/> (or a property or indexer access).
    /// </summary>
    public class Increment : PreUnaryOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Increment";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Increment"/> operator.
        /// </summary>
        public Increment(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// The internal name of the <see cref="UnaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return InternalName;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "++";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, true, Parse);
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

        protected Increment(Parser parser, CodeObject parent)
            : base(parser, parent, false)
        { }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        #endregion
    }
}
