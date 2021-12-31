using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational
{
    /// <summary>
    /// The LessThanEqual operator checks if the left Expression is less than or equal to the right Expression.
    /// </summary>
    public class LessThanEqual : RelationalOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "LessThanOrEqual";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "<=";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 330;

        /// <summary>
        /// Create a <see cref="LessThanEqual"/> operator.
        /// </summary>
        public LessThanEqual(Expression left, Expression right)
            : base(left, right)
        { }

        protected LessThanEqual(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="LessThanEqual"/> operator.
        /// </summary>
        public static LessThanEqual Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new LessThanEqual(parser, parent);
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
    }
}