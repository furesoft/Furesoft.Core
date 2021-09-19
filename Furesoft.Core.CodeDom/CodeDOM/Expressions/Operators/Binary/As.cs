using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary
{
    /// <summary>
    /// Converts an <see cref="Expression"/> to the specified type, returning null if the conversion can't be done.
    /// </summary>
    public class As : BinaryOperator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "as";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 330;

        /// <summary>
        /// Create an <see cref="As"/> operator.
        /// </summary>
        public As(Expression left, Expression type)
            : base(left, type)
        { }

        protected As(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            get { return false; }
        }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse an <see cref="As"/> operator.
        /// </summary>
        public static As Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new As(parser, parent);
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
