using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary
{
    /// <summary>
    /// Checks if an <see cref="Expression"/> is false.
    /// </summary>
    public class Not : PreUnaryOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Not";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Not"/> operator.
        /// </summary>
        public Not(Expression expression)
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
        public const string ParseToken = "!";

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
        /// Parse a <see cref="Not"/> operator.
        /// </summary>
        public static Not Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Not(parser, parent);
        }

        protected Not(Parser parser, CodeObject parent)
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

        #region /* RESOLVING */

        /// <summary>
        /// Perform a "not" operation on a constant object.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        /// <returns>
        /// The result, using the appropriate result type, including promoting smaller types to int. Returns null if the operation is invalid.
        /// </returns>
        protected override object EvaluateConstant(object constant)
        {
            // The only type supported for "!" is boolean
            if (constant is bool)
                return !(bool)constant;
            return null;  // The operation is invalid
        }

        #endregion
    }
}
