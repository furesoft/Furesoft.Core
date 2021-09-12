using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift.Base
{
    /// <summary>
    /// The common base class of <see cref="LeftShift"/> and <see cref="RightShift"/>.
    /// </summary>
    public abstract class BinaryShiftOperator : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        protected BinaryShiftOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PROPERTIES */

        #endregion

        #region /* PARSING */

        protected BinaryShiftOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Determine the types of both sides
            TypeRefBase leftTypeRefBase = (_left != null ? _left.EvaluateType(withoutConstants) : null);
            TypeRefBase rightTypeRefBase = (_right != null ? _right.EvaluateType(withoutConstants) : null);

            if (leftTypeRefBase is TypeRef && rightTypeRefBase is TypeRef)
            {
                TypeRef leftTypeRef = (TypeRef)leftTypeRefBase;
                TypeRef rightTypeRef = (TypeRef)rightTypeRefBase;

                // If both sides are constants, evaluate to a constant
                if (leftTypeRef.IsConst && rightTypeRef.IsConst)
                {
                    object result = EvaluateConstants(leftTypeRef.GetConstantValue(), rightTypeRef.GetConstantValue());
                    if (result != null)
                        return new TypeRef(result);
                }
            }

            // The resulting type of the shift is the type of the left operand
            return leftTypeRefBase;
        }

        #endregion
    }
}
