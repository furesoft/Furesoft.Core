using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Conditional
{
    /// <summary>
    /// Performs a conditional (logical) 'and' of two boolean <see cref="Expression"/>s.
    /// </summary>
    public class And : BinaryBooleanOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="And"/> operator.
        /// </summary>
        public And(Expression left, Expression right)
            : base(left, right)
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

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "&&";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 370;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="And"/> operator.
        /// </summary>
        public static And Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new And(parser, parent);
        }

        protected And(Parser parser, CodeObject parent)
            : base(parser, parent)
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
        /// Perform a logical AND of two constant objects.
        /// Supported types are: bool.
        /// </summary>
        protected override object EvaluateConstants(object leftConstant, object rightConstant)
        {
            if (leftConstant is bool && rightConstant is bool)
                return ((bool)leftConstant && (bool)rightConstant);
            return null;  // The operation is invalid
        }

        #endregion
    }
}
