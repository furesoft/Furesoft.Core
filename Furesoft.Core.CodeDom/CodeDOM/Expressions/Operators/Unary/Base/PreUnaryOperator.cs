using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base
{
    /// <summary>
    /// The common base class of all prefix unary operators (<see cref="Cast"/>, <see cref="Complement"/>,
    /// <see cref="Decrement"/>, <see cref="Increment"/>, <see cref="Negative"/>, <see cref="Not"/>, <see cref="Positive"/>).
    /// </summary>
    public abstract class PreUnaryOperator : UnaryOperator
    {
        #region /* CONSTRUCTORS */

        protected PreUnaryOperator(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        protected PreUnaryOperator(Parser parser, CodeObject parent, bool skipParsing)
            : base(parser, parent)
        {
            if (!skipParsing)
            {
                parser.NextToken();  // Skip past the operator
                SetField(ref _expression, Parse(parser, this), false);

                // Move any EOL or Postfix annotations from the expression up to the parent if there are no
                // parens in the way - this "normalizes" the annotations to the highest node on the line.
                if (_expression != null && _expression.HasEOLOrPostAnnotations && parent != parser.GetNormalizationBlocker())
                    MoveEOLAndPostAnnotations(_expression);
            }
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // If we have a reference to an overloaded operator declaration, use its return type
            if (_operatorRef is OperatorRef)
                return ((OperatorRef)_operatorRef).GetReturnType();

            // Determine the type of the expression
            TypeRefBase typeRefBase = (_expression != null ? _expression.EvaluateType(withoutConstants) : null);
            if (typeRefBase is TypeRef)
            {
                TypeRef typeRef = (TypeRef)typeRefBase;

                // Handle a constant
                if (typeRef.IsConst)
                {
                    object result = EvaluateConstant(typeRef.GetConstantValue());
                    return (result != null ? new TypeRef(result) : typeRefBase);
                }

                // String, boolean, or enum operands aren't supported by default, but we'll just return the
                // unchanged type instead of 'object'.
            }

            // By default, the type is unchanged
            return typeRefBase;
        }

        /// <summary>
        /// Evaluate the result of the operator on the specified constant value.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        /// <returns>
        /// The result, using the appropriate result type, including promoting smaller types to int. Returns null if the operation is invalid.
        /// </returns>
        protected virtual object EvaluateConstant(object constant)
        {
            // Assume the operation is invalid by default, which will result in the operation evaluating
            // to the type of the expression it operates on, with any constant value being lost.
            // This method will be overridden by operators that support constant values (Positive, Negative, Complement, Not).
            return null;
        }

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            AsTextOperator(writer, flags);
            if (_expression != null)
                _expression.AsText(writer, flags);
        }

        #endregion
    }
}
