// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The LessThan operator checks if the left Expression is less than the right Expression.
    /// </summary>
    public class LessThan : RelationalOperator
    {
        #region /* CONSTANTS */

        /// <summary>
        /// The internal name of the operator.
        /// </summary>
        public const string InternalName = NamePrefix + "LessThan";

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="LessThan"/> operator.
        /// </summary>
        public LessThan(Expression left, Expression right)
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
        public const string ParseToken = "<";

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
            // Use a parse-priority of 200 (GenericMethodDecl uses 0, UnresolvedRef uses 100)
            Parser.AddOperatorParsePoint(ParseToken, 200, Precedence, LeftAssociative, false, Parse);
        }

        /// <summary>
        /// Parse a <see cref="LessThan"/> operator.
        /// </summary>
        public static LessThan Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new LessThan(parser, parent);
        }

        protected LessThan(Parser parser, CodeObject parent)
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
    }
}
