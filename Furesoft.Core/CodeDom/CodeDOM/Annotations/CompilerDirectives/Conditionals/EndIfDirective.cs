// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Marks the end of an <see cref="IfDirective"/>.
    /// </summary>
    public class EndIfDirective : ConditionalDirectiveBase
    {
        /// <summary>
        /// Create an <see cref="EndIfDirective"/>.
        /// </summary>
        public EndIfDirective()
        { }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "endif";

        /// <summary>
        /// Parse an <see cref="EndIfDirective"/>.
        /// </summary>
        public EndIfDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'endif'
            MoveEOLComment(parser.LastToken);
            parser.EndConditionalDirective();
        }

        /// <summary>
        /// Parse an <see cref="EndIfDirective"/>.
        /// </summary>
        public static EndIfDirective Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new EndIfDirective(parser, parent);
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return false;
        }

        internal static void AddParsePoints()
        {
            Parser.AddCompilerDirectiveParsePoint(ParseToken, Parse);
        }

        /// <summary>
        /// True if the compiler directive has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return false; }
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public override string DirectiveKeyword
        {
            get { return ParseToken; }
        }
    }
}