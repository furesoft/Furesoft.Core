// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Resolving;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Checks if an <see cref="Expression"/> can be converted to the specified type, returning true if so.
    /// </summary>
    public class Is : BinaryBooleanOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="Is"/> operator.
        /// </summary>
        public Is(Expression left, Expression type)
            : base(left, type)
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
            get { return false; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "is";

        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public const int Precedence = 330;

        /// <summary>
        /// True if the operator is left-associative, or false if it's right-associative.
        /// </summary>
        public const bool LeftAssociative = true;

        internal static new void AddParsePoints()
        {
            Parser.AddOperatorParsePoint(ParseToken, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Is"/> operator.
        /// </summary>
        public static Is Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Is(parser, parent);
        }

        protected Is(Parser parser, CodeObject parent)
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
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_left != null)
                _left = (Expression)_left.Resolve(ResolveCategory.Expression, flags);

            // The decision was made to enforce that the right side of 'is' be a Type at resolution time
            // instead of during analysis, meaning that a non-type reference will end up being unresolved.
            // If we don't do this, a property with the same name as a type would take precedence.
            if (_right != null)
                _right = (Expression)_right.Resolve(ResolveCategory.Type, flags);

            return this;
        }

        #endregion
    }
}
