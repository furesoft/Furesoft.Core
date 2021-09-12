// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Arithmetic;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Methods;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Unary
{
    /// <summary>
    /// Negates an <see cref="Expression"/>.
    /// </summary>
    public class Negative : PreUnaryOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "UnaryNegation";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Negative"/> operator.
        /// </summary>
        public Negative(Expression expression)
            : base(expression)
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
        /// The internal name of the <see cref="UnaryOperator"/>.
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
        public const string ParseToken = Subtract.ParseToken;

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 200;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 100 (Subtract uses 0)
            Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, true, Parse);
        }

        /// <summary>
        /// Parse a <see cref="Negative"/> operator.
        /// </summary>
        public static Negative Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Negative(parser, parent);
        }

        protected Negative(Parser parser, CodeObject parent)
            : base(parser, parent, false)
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

            // Determine the type of the expression
            TypeRefBase typeRefBase = _expression.EvaluateType(withoutConstants);
            if (typeRefBase is TypeRef)
            {
                // Handle a constant
                if (typeRefBase.IsConst)
                {
                    object result = EvaluateConstant(typeRefBase.GetConstantValue());
                    return (result != null ? new TypeRef(result) : typeRefBase);
                }

                // String, boolean, or enum operands aren't supported by default, but we'll just return the
                // unchanged type instead of 'object'.

                // Do unary numeric promotions
                if (typeRefBase.IsSameRef(TypeRef.ShortRef) || typeRefBase.IsSameRef(TypeRef.UShortRef)
                    || typeRefBase.IsSameRef(TypeRef.SByteRef) || typeRefBase.IsSameRef(TypeRef.ByteRef) || typeRefBase.IsSameRef(TypeRef.CharRef))
                    return TypeRef.IntRef;
                if (typeRefBase.IsSameRef(TypeRef.UIntRef))
                    return TypeRef.LongRef;
            }

            // By default, the type is unchanged
            return typeRefBase;
        }

        /// <summary>
        /// Make a constant object negative.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        /// <returns>
        /// The result, using the appropriate result type, including promoting smaller types to int. Returns null if the operation is invalid.
        /// </returns>
        protected override object EvaluateConstant(object constant)
        {
            // Null, string, boolean, or enum operands aren't supported
            if (constant == null || constant is string || constant is bool || constant is EnumConstant)
                return null;  // The operation is invalid

            // Do unary numeric promotions
            if (constant is decimal)
                return -(decimal)constant;
            if (constant is double)
                return -(double)constant;
            if (constant is float)
                return -(float)constant;
            if (constant is ulong)
            {
                // Special case for long.MinValue
                if ((ulong)constant == 9223372036854775808)
                    return -9223372036854775808;
                return null;  // The operation is invalid
            }
            if (constant is long)
                return -(long)constant;
            if (constant is uint)
            {
                // Special case for int.MinValue
                if ((uint)constant == 2147483648)
                    return -2147483648;
                return -Convert.ToInt64(constant);
            }
            if (constant is int)
                return -(int)constant;
            // All other cases (with smaller integral types) get promoted to ints
            return -Convert.ToInt32(constant);
        }

        #endregion
    }
}
