using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise
{
    /// <summary>
    /// Performs a boolean OR operation on two <see cref="Expression"/>s.
    /// </summary>
    public class BitwiseOr : BinaryBitwiseOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "BitwiseOr";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "|";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 365;

        /// <summary>
        /// Create a <see cref="BitwiseOr"/> operator.
        /// </summary>
        public BitwiseOr(Expression left, Expression right)
            : base(left, right)
        { }

        protected BitwiseOr(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="BitwiseOr"/> operator.
        /// </summary>
        public static BitwiseOr Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new BitwiseOr(parser, parent);
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
