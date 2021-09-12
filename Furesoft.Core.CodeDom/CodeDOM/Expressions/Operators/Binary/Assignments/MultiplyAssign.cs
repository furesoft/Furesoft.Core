using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments
{
    /// <summary>
    /// Multiplies one <see cref="Expression"/> by another, and assigns the result to the left <see cref="Expression"/>.
    /// The left <see cref="Expression"/> must be an assignable object ("lvalue").
    /// </summary>
    public class MultiplyAssign : Assignment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="MultiplyAssign"/> operator.
        /// </summary>
        public MultiplyAssign(Expression left, Expression right)
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

        #region /* METHODS */

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return Multiply.InternalName;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "*=";

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="MultiplyAssign"/> operator.
        /// </summary>
        public static new MultiplyAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new MultiplyAssign(parser, parent);
        }

        protected MultiplyAssign(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
