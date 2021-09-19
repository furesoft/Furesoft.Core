using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Adds one <see cref="Expression"/> to another, and assigns the result to the left <see cref="Expression"/>.
    /// The left <see cref="Expression"/> must be an assignable object ("lvalue").
    /// </summary>
    public class AddAssign : Assignment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "+=";

        /// <summary>
        /// Create an <see cref="AddAssign"/> operator.
        /// </summary>
        public AddAssign(Expression left, Expression right)
            : base(left, right)
        { }

        /// <summary>
        /// Create an <see cref="AddAssign"/> operator.
        /// </summary>
        public AddAssign(EventDecl left, Expression right)
            : base(left.CreateRef(), right)
        { }

        protected AddAssign(Parser parser, CodeObject parent)
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
        /// Parse an <see cref="AddAssign"/> operator.
        /// </summary>
        public static new AddAssign Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new AddAssign(parser, parent);
        }

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return Add.InternalName;
        }

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }
    }
}