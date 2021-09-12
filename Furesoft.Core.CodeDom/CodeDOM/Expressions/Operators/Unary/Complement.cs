// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;

using Nova.Parsing;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Performs a one's complement (toggles all bits) on an <see cref="Expression"/>.
    /// </summary>
    public class Complement : PreUnaryOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "OnesComplement";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Complement"/> operator.
        /// </summary>
        public Complement(Expression expression)
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
        public const string ParseToken = "~";

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
            // Use a parse-priority of 100 (DestructorDecl uses 0)
            Parser.AddOperatorParsePoint(ParseToken, 100, Precedence, LeftAssociative, true, Parse);
        }

        /// <summary>
        /// Parse a <see cref="Complement"/>.
        /// </summary>
        public static Complement Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Complement(parser, parent);
        }

        protected Complement(Parser parser, CodeObject parent)
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
            }

            // By default, the type is unchanged
            return typeRefBase;
        }

        /// <summary>
        /// Perform a bit-wise complement of a constant object.
        /// Supported types are: string, bool, enum, decimal, double, float, ulong, long, uint, int, ushort, short, char, byte, sbyte.
        /// </summary>
        /// <returns>
        /// The result, using the appropriate result type, including promoting smaller types to int. Returns null if the operation is invalid.
        /// </returns>
        protected override object EvaluateConstant(object constant)
        {
            // Null, string, or boolean operands aren't supported
            if (constant == null || constant is string || constant is bool)
                return null;  // The operation is invalid

            // Handle enum constant
            if (constant is EnumConstant)
            {
                EnumConstant enumConstant = (EnumConstant)constant;
                object constantValue = enumConstant.ConstantValue;
                object enumValue = EvaluateConstant(constantValue);

                // If the calculated value was promoted to an int, convert it back to the original type
                if (enumValue is int && !(constantValue is int))
                    enumValue = TypeUtil.ChangeType(enumValue, constantValue.GetType());

                return new EnumConstant(enumConstant.EnumTypeRef, enumValue);
            }

            // Do unary numeric promotions
            if (constant is decimal || constant is double || constant is float)
                return null;  // The operation is invalid
            if (constant is ulong)
                return ~(ulong)constant;
            if (constant is long)
                return ~(long)constant;
            if (constant is uint)
                return ~(uint)constant;
            if (constant is int)
                return ~(int)constant;
            // All other cases (with smaller integral types) get promoted to ints
            return ~Convert.ToInt32(constant);
        }

        #endregion
    }
}
