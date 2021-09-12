// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Shift
{
    /// <summary>
    /// Shifts the bits of the left <see cref="Expression"/> to the RIGHT by the number of bits indicated by the right <see cref="Expression"/>.
    /// </summary>
    public class RightShift : BinaryShiftOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "RightShift";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="RightShift"/> operator.
        /// </summary>
        public RightShift(Expression left, Expression right)
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
        public const string ParseToken = ">>";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 320;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="RightShift"/> operator.
        /// </summary>
        public static RightShift Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new RightShift(parser, parent);
        }

        protected RightShift(Parser parser, CodeObject parent)
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
        /// Perform a right shift of a constant object by a constant amount.
        /// Supported left-side types are: enum, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// Right-side type should be 'int' with a 5 or 6 bit value.
        /// </summary>
        /// <returns>
        /// The left constant shifted right by the right constant amount, using the appropriate result type, including
        /// promoting smaller types to int. Returns null if the operation is invalid.
        /// </returns>
        protected override object EvaluateConstants(object leftConstant, object rightConstant)
        {
            // Check for null, string, and other invalid constants
            if (leftConstant == null || rightConstant == null || leftConstant is string || rightConstant is string
                || leftConstant is bool || rightConstant is bool || leftConstant is decimal || rightConstant is decimal
                || leftConstant is double || rightConstant is double || leftConstant is float || rightConstant is float)
                return null;  // The operation is invalid

            // Get the right shift count as an 'int'
            if ((rightConstant is ulong && ((ulong)rightConstant > int.MaxValue))
                || (rightConstant is long && ((long)rightConstant < 0 || (long)rightConstant > int.MaxValue))
                || (rightConstant is uint && ((uint)rightConstant > int.MaxValue))
                || (rightConstant is int && ((int)rightConstant < 0))
                || (rightConstant is short && ((short)rightConstant < 0))
                || (rightConstant is sbyte && ((sbyte)rightConstant < 0)))
                return null;  // The operation is invalid

            int right = (rightConstant is int ? (int)rightConstant : Convert.ToInt32(rightConstant));

            // Handle enum constant - implicitly convert an enum type on the left to its underlying type
            if (leftConstant is EnumConstant)
                leftConstant = ((EnumConstant)leftConstant).ConstantValue;

            if (leftConstant is ulong)
                return ((ulong)leftConstant >> right);
            if (leftConstant is long)
                return ((long)leftConstant >> right);
            if (leftConstant is uint)
                return ((uint)leftConstant >> right);
            if (leftConstant is int)
                return ((int)leftConstant >> right);
            // All other cases (with smaller integral types) get promoted to ints
            return (Convert.ToInt32(leftConstant) >> right);
        }

        #endregion
    }
}
