using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments
{
    /// <summary>
    /// Performs a boolean XOR operation on two <see cref="Expression"/>s, and assigns the result to the left <see cref="Expression"/>.
    /// The left <see cref="Expression"/> must be an assignable object ("lvalue").
    /// </summary>
    public class BitwiseXorAssign : Assignment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "^=";

        /// <summary>
        /// Create a <see cref="BitwiseXorAssign"/> operator.
        /// </summary>
        public BitwiseXorAssign(Expression left, Expression right)
            : base(left, right)
        { }

        protected BitwiseXorAssign(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        public static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="BitwiseXorAssign"/> operator.
        /// </summary>
        public static new BitwiseXorAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new BitwiseXorAssign(parser, parent);
        }

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return BitwiseXor.InternalName;
        }
    }
}