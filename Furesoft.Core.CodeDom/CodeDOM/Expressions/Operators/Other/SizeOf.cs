﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Runtime.InteropServices;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other
{
    /// <summary>
    /// Returns the size of the specified type.
    /// </summary>
    /// <remarks>
    /// This operator works only for value types, and can only be used in an unsafe context except
    /// for primitive integral types.  <see cref="Marshal.SizeOf(object)"/> can be used instead, but this size might
    /// vary from 'sizeof', because it won't include any padding used by the CLR.
    /// </remarks>
    public class SizeOf : TypeOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="SizeOf"/> operator.
        /// </summary>
        /// <param name="type">A TypeRef or an expression that evaluates to one.</param>
        public SizeOf(Expression type)
            : base(type)
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

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            get { return true; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "sizeof";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 100;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="SizeOf"/> operator.
        /// </summary>
        public static SizeOf Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new SizeOf(parser, parent);
        }

        protected SizeOf(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseKeywordAndArgument(parser, ParseFlags.Type);
        }

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
            // The following built-in types result in constant sizes outside of unsafe code blocks:
            TypeRefBase typeRefBase = _expression.EvaluateType(withoutConstants);
            if (typeRefBase.IsSameRef(TypeRef.BoolRef) || typeRefBase.IsSameRef(TypeRef.SByteRef) || typeRefBase.IsSameRef(TypeRef.ByteRef))
                return new TypeRef(TypeRef.IntRef, 1);
            if (typeRefBase.IsSameRef(TypeRef.CharRef) || typeRefBase.IsSameRef(TypeRef.ShortRef) || typeRefBase.IsSameRef(TypeRef.UShortRef))
                return new TypeRef(TypeRef.IntRef, 2);
            if (typeRefBase.IsSameRef(TypeRef.IntRef) || typeRefBase.IsSameRef(TypeRef.UIntRef) || typeRefBase.IsSameRef(TypeRef.FloatRef))
                return new TypeRef(TypeRef.IntRef, 4);
            if (typeRefBase.IsSameRef(TypeRef.LongRef) || typeRefBase.IsSameRef(TypeRef.ULongRef) || typeRefBase.IsSameRef(TypeRef.DoubleRef))
                return new TypeRef(TypeRef.IntRef, 8);
            if (typeRefBase.IsSameRef(TypeRef.DecimalRef))
                return new TypeRef(TypeRef.IntRef, 16);

            // Otherwise, just return 'int'
            return TypeRef.IntRef;
        }

        #endregion
    }
}
