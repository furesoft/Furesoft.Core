// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Bitwise
{
    /// <summary>
    /// Performs a boolean OR operation on two <see cref="Expression"/>s.
    /// </summary>
    public class BitwiseOr : BinaryBitwiseOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "BitwiseOr";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="BitwiseOr"/> operator.
        /// </summary>
        public BitwiseOr(Expression left, Expression right)
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
        public const string ParseToken = "|";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 365;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="BitwiseOr"/> operator.
        /// </summary>
        public static BitwiseOr Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new BitwiseOr(parser, parent);
        }

        protected BitwiseOr(Parser parser, CodeObject parent)
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
        /// Perform a bitwise OR of two constant objects.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        /// <returns>
        /// The bitwise OR of the two constants, using the appropriate result type, including promoting smaller
        /// types to int. Returns null if the operation is invalid.
        /// </returns>
        protected override object EvaluateConstants(object leftConstant, object rightConstant)
        {
            // Check for null or string constants
            if (leftConstant == null || rightConstant == null || leftConstant is string || rightConstant is string)
                return null;  // The operation is invalid

            // Handle bool constants
            if (leftConstant is bool || rightConstant is bool)
            {
                if (leftConstant is bool && rightConstant is bool)
                    return (bool)leftConstant | (bool)rightConstant;
                return null;  // The operation is invalid
            }

            // Handle enum constants
            if (leftConstant is EnumConstant || rightConstant is EnumConstant)
            {
                // If both sides are enums, the result is an enum.  If only one side is an enum, convert the
                // enum operand to its constant value for the numeric calculations below.
                if (leftConstant is EnumConstant)
                {
                    EnumConstant leftEnum = (EnumConstant)leftConstant;
                    if (rightConstant is EnumConstant)
                    {
                        EnumConstant rightEnum = (EnumConstant)rightConstant;
                        if (leftEnum.EnumTypeRef.IsSameRef(rightEnum.EnumTypeRef))
                            return new EnumConstant(leftEnum.EnumTypeRef, EvaluateConstants(leftEnum.ConstantValue, rightEnum.ConstantValue));
                        return null;
                    }
                    leftConstant = leftEnum.ConstantValue;
                }
                else
                    rightConstant = ((EnumConstant)rightConstant).ConstantValue;
            }

            // Do binary numeric promotions
            if (leftConstant is decimal || rightConstant is decimal || leftConstant is double || rightConstant is double
                || leftConstant is float || rightConstant is float)
                return null;  // The operation is invalid
            if (leftConstant is ulong)
            {
                if ((rightConstant is sbyte && (sbyte)rightConstant < 0) || (rightConstant is short && (short)rightConstant < 0)
                    || (rightConstant is int && (int)rightConstant < 0) || (rightConstant is long && (long)rightConstant < 0))
                    return null;  // The operation is invalid
                return (ulong)leftConstant | Convert.ToUInt64(rightConstant);
            }
            if (rightConstant is ulong)
            {
                if ((leftConstant is sbyte && (sbyte)leftConstant < 0) || (leftConstant is short && (short)leftConstant < 0)
                    || (leftConstant is int && (int)leftConstant < 0) || (leftConstant is long && (long)leftConstant < 0))
                    return null;  // The operation is invalid
                return Convert.ToUInt64(leftConstant) | (ulong)rightConstant;
            }
            if (leftConstant is long)
                return (long)leftConstant | Convert.ToInt64(rightConstant);
            if (rightConstant is long)
                return Convert.ToInt64(leftConstant) | (long)rightConstant;
            if (leftConstant is uint)
            {
                if ((rightConstant is sbyte && (sbyte)rightConstant < 0) || (rightConstant is short && (short)rightConstant < 0)
                    || (rightConstant is int && (int)rightConstant < 0))
                    return Convert.ToInt64(leftConstant) | Convert.ToInt64(rightConstant);
                return (uint)leftConstant | Convert.ToUInt32(rightConstant);
            }
            if (rightConstant is uint)
            {
                if ((leftConstant is sbyte && (sbyte)leftConstant < 0) || (leftConstant is short && (short)leftConstant < 0)
                    || (leftConstant is int && (int)leftConstant < 0))
                    return Convert.ToInt64(leftConstant) | Convert.ToInt64(rightConstant);
                return Convert.ToUInt32(leftConstant) | (uint)rightConstant;
            }
            // All other cases (with smaller integral types) get promoted to ints
            return Convert.ToInt32(leftConstant) | Convert.ToInt32(rightConstant);
        }

        #endregion
    }
}
