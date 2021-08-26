// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Forces the compiler to emit a warning message.
    /// </summary>
    public class WarningDirective : MessageDirective
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="WarningDirective"/>.
        /// </summary>
        public WarningDirective(string message)
            : base(message)
        { }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "warning";

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="WarningDirective"/>.
        /// </summary>
        public static WarningDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new WarningDirective(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="WarningDirective"/>.
        /// </summary>
        public WarningDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseMessage(parser);
        }

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
