// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary bitwise operators (<see cref="BitwiseAnd"/>, <see cref="BitwiseOr"/>, <see cref="BitwiseXor"/>).
    /// </summary>
    public abstract class BinaryBitwiseOperator : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        protected BinaryBitwiseOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PARSING */

        protected BinaryBitwiseOperator(Parser parser, CodeObject parent)
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
            // If we have a reference to an overloaded operator declaration, use its return type
            if (_operatorRef is OperatorRef)
                return ((OperatorRef)_operatorRef).GetReturnType();

            // Determine the types of both sides
            TypeRefBase leftTypeRefBase = (_left != null ? _left.EvaluateType(withoutConstants) : null);
            TypeRefBase rightTypeRefBase = (_right != null ? _right.EvaluateType(withoutConstants) : null);
            if (leftTypeRefBase is TypeRef && rightTypeRefBase is TypeRef)
            {
                TypeRef leftTypeRef = (TypeRef)leftTypeRefBase;
                TypeRef rightTypeRef = (TypeRef)rightTypeRefBase;

                // Handle constants
                if (leftTypeRef.IsConst && rightTypeRef.IsConst)
                {
                    object result = EvaluateConstants(leftTypeRef.GetConstantValue(), rightTypeRef.GetConstantValue());
                    return (result != null ? new TypeRef(result) : TypeRef.ObjectRef);
                }

                // String operands aren't supported for bitwise operations
                if (leftTypeRef.IsSameRef(TypeRef.StringRef) || rightTypeRef.IsSameRef(TypeRef.StringRef))
                    return null;

                // Boolean operands are valid and give a boolean result
                if (leftTypeRef.IsSameRef(TypeRef.BoolRef) && rightTypeRef.IsSameRef(TypeRef.BoolRef))
                    return TypeRef.BoolRef;

                // Enums are supported if they're the same type - they should also be bit-flag enums, but
                // we'll allow that to pass here and rely on the analysis logic to warn if they aren't.
                if (leftTypeRef.IsEnum && rightTypeRef.IsEnum && leftTypeRef.IsSameRef(rightTypeRef))
                    return (!leftTypeRef.IsConst ? leftTypeRef : (!rightTypeRef.IsConst ? rightTypeRef : leftTypeRef.GetTypeWithoutConstant()));
            }

            // By default, determine a common type (using implicit conversions) that can handle the
            // result of the operation.
            return TypeRef.GetCommonType(leftTypeRefBase, rightTypeRefBase);
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }  // Default to NO parens for binary bitwise operators
        }

        #endregion
    }
}
