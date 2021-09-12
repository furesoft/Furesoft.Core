using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary
{
    /// <summary>
    /// Returns the right <see cref="Expression"/> if the left <see cref="Expression"/> is null,
    /// otherwise it returns the left <see cref="Expression"/>.
    /// </summary>
    public class IfNullThen : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="IfNullThen"/> operator.
        /// </summary>
        public IfNullThen(Expression left, Expression right)
            : base(left, right)
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
        public const string ParseToken = "??";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 390;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = false;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="IfNullThen"/> operator.
        /// </summary>
        public static IfNullThen Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new IfNullThen(parser, parent);
        }

        protected IfNullThen(Parser parser, CodeObject parent)
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
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Type evaluation rules
            // - If A exists and is not a nullable type or a reference type, a compile-time error occurs.
            TypeRefBase leftType = (_left != null ? _left.EvaluateType(withoutConstants) : null);
            if (leftType == null || leftType is UnresolvedRef || (!leftType.IsNullableType && (!(leftType is TypeRef) || ((TypeRef)leftType).IsValueType)))
                return leftType;  // Just evaluate to the left type, even though it's invalid and analysis will flag an error
            TypeRefBase rightType = (_right != null ? _right.EvaluateType(withoutConstants) : null);
            // - Otherwise, if A exists and is a nullable type and an implicit conversion exists from b to A0, the result type is A0.
            //   At run-time, a is first evaluated. If a is not null, a is unwrapped to type A0, and this becomes the result.
            //   Otherwise, b is evaluated and converted to type A0, and this becomes the result.
            // - Otherwise, if A exists and an implicit conversion exists from b to A, the result type is A. At run-time, a is first evaluated.
            //   If a is not null, a becomes the result. Otherwise, b is evaluated and converted to type A, and this becomes the result.
            if (leftType is TypeRef && rightType != null)
            {
                if (leftType.IsNullableType)
                {
                    TypeRefBase A0 = leftType.TypeArguments[0].EvaluateType(withoutConstants);
                    if (rightType.IsImplicitlyConvertibleTo(A0))
                        return A0;
                }
                if (rightType.IsImplicitlyConvertibleTo(leftType))
                    return leftType;
            }
            // - Otherwise, if b has a type B and an implicit conversion exists from a to B, the result type is B. At run-time, a is first evaluated.
            //   If a is not null, a is unwrapped to type A0 (if A exists and is nullable) and converted to type B, and this becomes the result.
            //   Otherwise, b is evaluated and becomes the result.
            if (leftType.IsImplicitlyConvertibleTo(rightType))
                return rightType;
            // - Otherwise, a and b are incompatible, and a compile-time error occurs.
            return leftType;  // Just evaluate to the left type, even though it's invalid and analysis will flag an error
        }

        #endregion
    }
}
