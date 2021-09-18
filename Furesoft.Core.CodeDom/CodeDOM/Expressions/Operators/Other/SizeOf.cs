// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Runtime.InteropServices;

using Nova.Parsing;

namespace Nova.CodeDOM
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
    }
}
