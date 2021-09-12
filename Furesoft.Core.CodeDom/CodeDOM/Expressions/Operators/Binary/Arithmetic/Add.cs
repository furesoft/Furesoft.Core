// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Adds one <see cref="Expression"/> to another.
    /// </summary>
    public class Add : BinaryArithmeticOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Addition";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Add"/> operator.
        /// </summary>
        public Add(Expression left, Expression right)
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

        #endregion

        #region /* METHODS */

        /// <summary>
        /// The internal name of the <see cref="BinaryOperator"/>.
        /// </summary>
        public override string GetInternalName()
        {
            return InternalName;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "+";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 310;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Add"/> operator.
        /// </summary>
        public static Add Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            // Verify that we have a left expression before proceeding, otherwise abort
            // (this is to give the Positive operator a chance at parsing it)
            if (parser.HasUnusedExpression)
                return new Add(parser, parent);
            return null;
        }

        protected Add(Parser parser, CodeObject parent)
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
                if (!withoutConstants && leftTypeRef.IsConst && rightTypeRef.IsConst)
                {
                    object result = EvaluateConstants(leftTypeRef.GetConstantValue(), rightTypeRef.GetConstantValue());
                    return (result != null ? new TypeRef(result) : TypeRef.ObjectRef);
                }

                // Evaluate as a string if either side is a string OR the left side is implicitly convertible to a string (string concatenation)
                if (leftTypeRef.IsSameRef(TypeRef.StringRef) || rightTypeRef.IsSameRef(TypeRef.StringRef)
                    || leftTypeRef.UserDefinedImplicitConversionExists(TypeRef.StringRef))
                    return TypeRef.StringRef;

                // Boolean operands aren't supported
                if (leftTypeRef.IsSameRef(TypeRef.BoolRef) || rightTypeRef.IsSameRef(TypeRef.BoolRef))
                    return null;

                // Enumeration addition: If one side is an enum and the other is of the underlying type
                // of the enum (or is implicitly convertible to it), then the result is the enum type.
                // E +(E, U):
                if (leftTypeRef.IsEnum)
                    return (rightTypeRef.IsImplicitlyConvertibleTo(leftTypeRef.GetUnderlyingTypeOfEnum()) ? leftTypeRef : TypeRef.ObjectRef);
                // E +(U, E):
                if (rightTypeRef.IsEnum)
                    return (leftTypeRef.IsImplicitlyConvertibleTo(rightTypeRef.GetUnderlyingTypeOfEnum()) ? rightTypeRef : TypeRef.ObjectRef);
            }
            else if (leftTypeRefBase != null && rightTypeRefBase != null)
            {
                // Evaluate as a string if one side is unresolved and the other is a string (reduce error propagation)
                if (leftTypeRefBase.IsSameRef(TypeRef.StringRef) || rightTypeRefBase.IsSameRef(TypeRef.StringRef))
                    return TypeRef.StringRef;
            }

            // By default, determine a common type (using implicit conversions) that can handle the
            // result of the operation.
            return TypeRef.GetCommonType(leftTypeRefBase, rightTypeRefBase);
        }

        /// <summary>
        /// Add two constant objects.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        /// <returns>
        /// The sum of the two constants, using the appropriate result type, including promoting smaller
        /// types to int. Returns null if the operation is invalid.
        /// </returns>
        protected override object EvaluateConstants(object leftConstant, object rightConstant)
        {
            // Do string concatenation if either side is a string (even if the other side is null)
            if (leftConstant is string)
                return ((string)leftConstant + rightConstant);
            if (rightConstant is string)
                return (leftConstant + (string)rightConstant);

            // Check for null constants
            if (leftConstant == null || rightConstant == null)
                return null;  // The operation is invalid

            // Boolean operands aren't supported
            if (leftConstant is bool || rightConstant is bool)
                return null;  // The operation is invalid

            // Handle enum constants
            // Enumeration addition: If one side is an enum and the other is of the underlying type
            // of the enum (or is implicitly convertible to it), then the result is the enum type.
            if (leftConstant is EnumConstant)
            {
                if (!(rightConstant is EnumConstant))
                {
                    EnumConstant leftEnumConstant = (EnumConstant)leftConstant;
                    TypeRefBase underlyingTypeRefBase = leftEnumConstant.EnumTypeRef.GetUnderlyingTypeOfEnum();
                    TypeRef rightTypeRef = new TypeRef(rightConstant.GetType());
                    if (rightTypeRef.IsImplicitlyConvertibleTo(underlyingTypeRefBase))
                    {
                        // E +(E, U):
                        object enumValue = EvaluateConstants(leftEnumConstant.ConstantValue, rightConstant);
                        if (enumValue != null)
                            return new EnumConstant(leftEnumConstant.EnumTypeRef, TypeRef.ChangeTypeOfConstant(enumValue, underlyingTypeRefBase));
                    }
                }
                return null;  // The operation is invalid
            }
            if (rightConstant is EnumConstant)
            {
                EnumConstant rightEnumConstant = (EnumConstant)rightConstant;
                TypeRefBase underlyingTypeRefBase = rightEnumConstant.EnumTypeRef.GetUnderlyingTypeOfEnum();
                TypeRef leftTypeRef = new TypeRef(leftConstant.GetType());
                if (leftTypeRef.IsImplicitlyConvertibleTo(underlyingTypeRefBase))
                {
                    // E +(U, E):
                    object enumValue = EvaluateConstants(leftConstant, rightEnumConstant.ConstantValue);
                    if (enumValue != null)
                        return new EnumConstant(rightEnumConstant.EnumTypeRef, TypeRef.ChangeTypeOfConstant(enumValue, underlyingTypeRefBase));
                }
                return null;  // The operation is invalid
            }

            // Do binary numeric promotions
            if (leftConstant is decimal)
            {
                if (rightConstant is float || rightConstant is double)
                    return null;  // The operation is invalid
                return (decimal)leftConstant + Convert.ToDecimal(rightConstant);
            }
            if (rightConstant is decimal)
            {
                if (leftConstant is float || leftConstant is double)
                    return null;  // The operation is invalid
                return Convert.ToDecimal(leftConstant) + (decimal)rightConstant;
            }
            if (leftConstant is double)
                return (double)leftConstant + Convert.ToDouble(rightConstant);
            if (rightConstant is double)
                return Convert.ToDouble(leftConstant) + (double)rightConstant;
            if (leftConstant is float)
                return (float)leftConstant + Convert.ToSingle(rightConstant);
            if (rightConstant is float)
                return Convert.ToSingle(leftConstant) + (float)rightConstant;
            if (leftConstant is ulong)
            {
                if ((rightConstant is sbyte && (sbyte)rightConstant < 0) || (rightConstant is short && (short)rightConstant < 0)
                    || (rightConstant is int && (int)rightConstant < 0) || (rightConstant is long && (long)rightConstant < 0))
                    return null;  // The operation is invalid
                return (ulong)leftConstant + Convert.ToUInt64(rightConstant);
            }
            if (rightConstant is ulong)
            {
                if ((leftConstant is sbyte && (sbyte)leftConstant < 0) || (leftConstant is short && (short)leftConstant < 0)
                    || (leftConstant is int && (int)leftConstant < 0) || (leftConstant is long && (long)leftConstant < 0))
                    return null;  // The operation is invalid
                return Convert.ToUInt64(leftConstant) + (ulong)rightConstant;
            }
            if (leftConstant is long)
                return (long)leftConstant + Convert.ToInt64(rightConstant);
            if (rightConstant is long)
                return Convert.ToInt64(leftConstant) + (long)rightConstant;
            if (leftConstant is uint)
            {
                if ((rightConstant is sbyte && (sbyte)rightConstant < 0) || (rightConstant is short && (short)rightConstant < 0)
                    || (rightConstant is int && (int)rightConstant < 0))
                    return Convert.ToInt64(leftConstant) + Convert.ToInt64(rightConstant);
                return (uint)leftConstant + Convert.ToUInt32(rightConstant);
            }
            if (rightConstant is uint)
            {
                if ((leftConstant is sbyte && (sbyte)leftConstant < 0) || (leftConstant is short && (short)leftConstant < 0)
                    || (leftConstant is int && (int)leftConstant < 0))
                    return Convert.ToInt64(leftConstant) + Convert.ToInt64(rightConstant);
                return Convert.ToUInt32(leftConstant) + (uint)rightConstant;
            }
            // All other cases (with smaller integral types) get promoted to ints
            return Convert.ToInt32(leftConstant) + Convert.ToInt32(rightConstant);
        }

        #endregion
    }
}
