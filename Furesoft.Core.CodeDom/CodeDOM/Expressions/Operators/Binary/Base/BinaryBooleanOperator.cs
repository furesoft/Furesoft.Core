// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary operators that evaluate to boolean values (<see cref="RelationalOperator"/>
    /// [common base of <see cref="Equal"/>, <see cref="NotEqual"/>, <see cref="GreaterThan"/>, <see cref="LessThan"/>,
    /// <see cref="GreaterThanEqual"/>, <see cref="LessThanEqual"/>], <see cref="And"/>, <see cref="Or"/>, <see cref="Is"/>).
    /// </summary>
    public abstract class BinaryBooleanOperator : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        protected BinaryBooleanOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PARSING */

        protected BinaryBooleanOperator(Parser parser, CodeObject parent)
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

            // Handle constants - returning a constant 'true' or 'false' if possible
            TypeRef leftTypeRef = (_left != null ? _left.EvaluateType(withoutConstants) as TypeRef : null);
            TypeRef rightTypeRef = (_right != null ? _right.EvaluateType(withoutConstants) as TypeRef : null);
            if (leftTypeRef != null && rightTypeRef != null && leftTypeRef.IsConst && rightTypeRef.IsConst)
            {
                object result = EvaluateConstants(leftTypeRef.GetConstantValue(), rightTypeRef.GetConstantValue());
                if (result != null)
                    return new TypeRef(result);
            }

            // Don't bother checking for valid uses of relational operators with strings, bools, and
            // enums - just assume a bool return type.

            // Default to returning a 'bool' type
            return TypeRef.BoolRef;
        }

        #endregion
    }
}
