using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary
{
    /// <summary>
    /// Checks if an <see cref="Expression"/> can be converted to the specified type, returning true if so.
    /// </summary>
    public class Is : BinaryBooleanOperator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "is";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 330;

        /// <summary>
        /// Create an <see cref="Is"/> operator.
        /// </summary>
        public Is(Expression left, Expression type)
            : base(left, type)
        { }

        protected Is(Parser parser, CodeObject parent)
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
        /// Parse an <see cref="Is"/> operator.
        /// </summary>
        public static Is Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Is(parser, parent);
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
