using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary
{
    /// <summary>
    /// Converts an <see cref="Expression"/> to the specified type, returning null if the conversion can't be done.
    /// </summary>
    public class As : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="As"/> operator.
        /// </summary>
        public As(Expression left, Expression type)
            : base(left, type)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The symbol associated with the operator.
        /// </summary>
        public override string Symbol
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            get { return false; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "as";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 330;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="As"/> operator.
        /// </summary>
        public static As Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new As(parser, parent);
        }

        protected As(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        /// <summary>
        /// Get the precedence of the operator.
        /// </summary>
        public override int GetPrecedence()
        {
            return Precedence;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            // Override the base-class behavior so that we can resolve the right side as a Type
            // instead of an Expression (avoiding matches on Property names, for example).
            if (_left != null)
                _left = (Expression)_left.Resolve(ResolveCategory.Expression, flags);
            if (_right != null)
                _right = (Expression)_right.Resolve(ResolveCategory.Type, flags);
            return this;
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Evaluate to the type specified by the Right expression
            return _right.EvaluateType(withoutConstants);
        }

        #endregion
    }
}
