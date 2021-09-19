using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Returns the remainder of the division of one <see cref="Expression"/> by another.
    /// </summary>
    public class Mod : BinaryArithmeticOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Modulus";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "%";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 300;

        /// <summary>
        /// Create a <see cref="Mod"/> operator.
        /// </summary>
        public Mod(Expression left, Expression right)
            : base(left, right)
        { }

        protected Mod(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="Mod"/> operator.
        /// </summary>
        public static Mod Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Mod(parser, parent);
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