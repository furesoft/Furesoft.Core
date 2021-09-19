using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Explicitly turns OFF overflow checking (exceptions) for an <see cref="Expression"/>.
    /// </summary>
    public class Unchecked : CheckedOperator
    {
        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "unchecked";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        /// <summary>
        /// Create an <see cref="Unchecked"/> operator.
        /// </summary>
        public Unchecked(Expression expression)
            : base(expression)
        { }

        protected Unchecked(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            ParseKeywordAndArgument(parser, ParseFlags.NotAType);
        }

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Parse an <see cref="Unchecked"/> operator.
        /// </summary>
        public static Unchecked Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Unchecked(parser, parent);
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
            // Use a parse-priority of 100 (UncheckedBlock uses 0)
            Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, false, Parse);
        }
    }
}