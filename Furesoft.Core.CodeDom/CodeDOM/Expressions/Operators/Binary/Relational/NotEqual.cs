﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The NotEqual operator checks if the left Expression is NOT equal to the right Expression.
    /// </summary>
    public class NotEqual : RelationalOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "Inequality";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="NotEqual"/> operator.
        /// </summary>
        public NotEqual(Expression left, Expression right)
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
        public const string ParseToken = "!=";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 340;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="NotEqual"/> operator.
        /// </summary>
        public static NotEqual Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new NotEqual(parser, parent);
        }

        protected NotEqual(Parser parser, CodeObject parent)
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
        /// Compare if the first constant object is not equal to the second.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        protected override object EvaluateConstants(object leftConstant, object rightConstant)
        {
            // Check for null constants
            if (leftConstant == null || rightConstant == null)
            {
                if (leftConstant == null && rightConstant == null)
                    return false;
                return null;  // The operation is invalid
            }

            // Check for string, bool, or enum types
            if (leftConstant is string || rightConstant is string)
            {
                if (leftConstant is string && rightConstant is string)
                    return ((string)leftConstant != (string)rightConstant);
                return null;  // The operation is invalid
            }
            if (leftConstant is bool || rightConstant is bool)
            {
                if (leftConstant is bool && rightConstant is bool)
                    return ((bool)leftConstant != (bool)rightConstant);
                return null;  // The operation is invalid
            }
            if (leftConstant is EnumConstant || rightConstant is EnumConstant)
            {
                if (leftConstant is EnumConstant && rightConstant is EnumConstant)
                {
                    EnumConstant leftEnum = (EnumConstant)leftConstant;
                    EnumConstant rightEnum = (EnumConstant)rightConstant;
                    if (leftEnum.EnumTypeRef.IsSameRef(rightEnum.EnumTypeRef))
                        return EvaluateConstants(leftEnum.ConstantValue, rightEnum.ConstantValue);
                }
                return null;  // The operation is invalid
            }

            // Do binary numeric promotions
            if (leftConstant is decimal)
            {
                if (!(rightConstant is float || rightConstant is double))
                    return (decimal)leftConstant != Convert.ToDecimal(rightConstant);
                return null;  // The operation is invalid
            }
            if (rightConstant is decimal)
            {
                if (!(leftConstant is float || leftConstant is double))
                    return Convert.ToDecimal(leftConstant) != (decimal)rightConstant;
                return null;  // The operation is invalid
            }
            if (leftConstant is double)
                return (double)leftConstant != Convert.ToDouble(rightConstant);
            if (rightConstant is double)
                return Convert.ToDouble(leftConstant) != (double)rightConstant;
            if (leftConstant is float)
                return (float)leftConstant != Convert.ToSingle(rightConstant);
            if (rightConstant is float)
                return Convert.ToSingle(leftConstant) != (float)rightConstant;
            if (leftConstant is ulong)
            {
                if ((rightConstant is sbyte && (sbyte)rightConstant < 0) || (rightConstant is short && (short)rightConstant < 0)
                    || (rightConstant is int && (int)rightConstant < 0) || (rightConstant is long && (long)rightConstant < 0))
                    return null;  // The operation is invalid
                return (ulong)leftConstant != Convert.ToUInt64(rightConstant);
            }
            if (rightConstant is ulong)
            {
                if ((leftConstant is sbyte && (sbyte)leftConstant < 0) || (leftConstant is short && (short)leftConstant < 0)
                    || (leftConstant is int && (int)leftConstant < 0) || (leftConstant is long && (long)leftConstant < 0))
                    return null;  // The operation is invalid
                return Convert.ToUInt64(leftConstant) != (ulong)rightConstant;
            }
            if (leftConstant is long)
                return (long)leftConstant != Convert.ToInt64(rightConstant);
            if (rightConstant is long)
                return Convert.ToInt64(leftConstant) != (long)rightConstant;
            if (leftConstant is uint)
            {
                if ((rightConstant is sbyte && (sbyte)rightConstant < 0) || (rightConstant is short && (short)rightConstant < 0)
                    || (rightConstant is int && (int)rightConstant < 0))
                    return Convert.ToInt64(leftConstant) != Convert.ToInt64(rightConstant);
                return (uint)leftConstant != Convert.ToUInt32(rightConstant);
            }
            if (rightConstant is uint)
            {
                if ((leftConstant is sbyte && (sbyte)leftConstant < 0) || (leftConstant is short && (short)leftConstant < 0)
                    || (leftConstant is int && (int)leftConstant < 0))
                    return Convert.ToInt64(leftConstant) != Convert.ToInt64(rightConstant);
                return Convert.ToUInt32(leftConstant) != (uint)rightConstant;
            }
            // All other cases (with smaller integral types) get promoted to ints
            return Convert.ToInt32(leftConstant) != Convert.ToInt32(rightConstant);
        }

        #endregion
    }
}
