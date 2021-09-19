using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational
{
    /// <summary>
    /// The LessThan operator checks if the left Expression is less than the right Expression.
    /// </summary>
    public class LessThan : RelationalOperator
    {
        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "LessThan";

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "<";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 330;

        /// <summary>
        /// Create a <see cref="LessThan"/> operator.
        /// </summary>
        public LessThan(Expression left, Expression right)
            : base(left, right)
        { }

        protected LessThan(Parser parser, CodeObject parent)
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
        /// Parse a <see cref="LessThan"/> operator.
        /// </summary>
        public static LessThan Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new LessThan(parser, parent);
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
            // Use a parse-priority of 200 (GenericMethodDecl uses 0, UnresolvedRef uses 100)
            Parser.AddOperatorParsePoint(ParseToken, 200, Precedence, LeftAssociative, false, Parse);
        }
    }
}
