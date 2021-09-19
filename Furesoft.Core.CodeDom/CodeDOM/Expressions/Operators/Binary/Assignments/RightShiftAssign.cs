using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments
{
    /// <summary>
    /// Shifts the value of the left <see cref="Expression"/> RIGHT by the value of the right <see cref="Expression"/>, and assigns the
    /// result to the left <see cref="Expression"/>.  The left <see cref="Expression"/> must be an assignable object ("lvalue").
    /// </summary>
    public class RightShiftAssign : Assignment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = ">>=";

        /// <summary>
        /// Create a <see cref="RightShiftAssign"/> operator.
        /// </summary>
        public RightShiftAssign(Expression left, Expression right)
            : base(left, right)
        { }

        protected RightShiftAssign(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="RightShiftAssign"/> operator.
        /// </summary>
        public static new RightShiftAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new RightShiftAssign(parser, parent);
        }

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return RightShift.InternalName;
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}
