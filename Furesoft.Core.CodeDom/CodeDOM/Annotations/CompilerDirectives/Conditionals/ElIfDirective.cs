// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Used for optional compilation of code, must be preceeded by an <see cref="IfDirective"/> or <see cref="ElIfDirective"/>, and
    /// followed by one of <see cref="ElIfDirective"/>, <see cref="ElseDirective"/>, or <see cref="EndIfDirective"/>.
    /// </summary>
    public class ElIfDirective : ConditionalExpressionDirective
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create an <see cref="ElIfDirective"/> with the specified <see cref="Expression"/>.
        /// </summary>
        public ElIfDirective(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "elif";

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse an <see cref="ElIfDirective"/>.
        /// </summary>
        public static ElIfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new ElIfDirective(parser, parent);
        }

        /// <summary>
        /// Parse an <see cref="ElIfDirective"/>.
        /// </summary>
        public ElIfDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* RENDERING */

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }

        #endregion
    }
}
