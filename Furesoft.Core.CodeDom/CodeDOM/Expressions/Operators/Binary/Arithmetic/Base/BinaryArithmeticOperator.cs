// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary arithmetic operators (<see cref="Add"/>, <see cref="Subtract"/>, <see cref="Multiply"/>, <see cref="Divide"/>, <see cref="Mod"/>).
    /// </summary>
    public abstract class BinaryArithmeticOperator : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        protected BinaryArithmeticOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PARSING */

        protected BinaryArithmeticOperator(Parser parser, CodeObject parent)
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

                // String, boolean, or enum operands aren't supported by default (Add and Subtract override this method)
                if (leftTypeRef.IsSameRef(TypeRef.StringRef) || rightTypeRef.IsSameRef(TypeRef.StringRef)
                    || leftTypeRef.IsSameRef(TypeRef.BoolRef) || rightTypeRef.IsSameRef(TypeRef.BoolRef) || leftTypeRef.IsEnum || rightTypeRef.IsEnum)
                    return null;
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
            get { return false; }  // Default to NO parens for binary arithmetic operators
        }

        #endregion
    }
}
