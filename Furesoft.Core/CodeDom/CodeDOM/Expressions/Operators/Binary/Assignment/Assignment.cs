// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Assigns the right <see cref="Expression"/> to the left <see cref="Expression"/>.
    /// Also the common base class of all compound assignment operators (<see cref="AddAssign"/>, <see cref="BitwiseAndAssign"/>,
    /// <see cref="BitwiseOrAssign"/>, <see cref="BitwiseXorAssign"/>, <see cref="DivideAssign"/>, <see cref="LeftShiftAssign"/>,
    /// <see cref="ModAssign"/>, <see cref="MultiplyAssign"/>, <see cref="RightShiftAssign"/>, <see cref="SubtractAssign"/>).
    /// </summary>
    /// <remarks>
    /// The left <see cref="Expression"/> must be an assignable object ("lvalue").
    /// </remarks>
    public class Assignment : BinaryOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Assignment"/> with the specified left and right expressions.
        /// </summary>
        public Assignment(Expression left, Expression right)
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

        /// <summary>
        /// True if the expression is const.
        /// </summary>
        public override bool IsConst
        {
            // The result of an assignment is never const, because the lvalue must be a variable
            get { return false; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "=";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 500;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = false;

        internal static new void AddParsePoints()
        {
            // Use a parse-priority of 300 (FieldDecl uses 0, LocalDecl uses 100, MultiEnumMemberDecl uses 200)
            Parser.AddOperatorParsePoint(ParseToken, 300, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Assignment"/>.
        /// </summary>
        public static Assignment Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Assignment(parser, parent);
        }

        protected Assignment(Parser parser, CodeObject parent)
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

        #region /* FORMATTING */

        /// <summary>
        /// True if the expression should have parens by default.
        /// </summary>
        public override bool HasParensDefault
        {
            get { return false; }  // No parens for assignments by default
        }

        #endregion
    }
}
