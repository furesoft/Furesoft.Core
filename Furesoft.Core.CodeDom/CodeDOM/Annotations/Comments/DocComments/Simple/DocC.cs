// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a fragment of code in a documentation comment.
    /// </summary>
    public class DocC : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "c";

        /// <summary>
        /// Create a <see cref="DocC"/>.
        /// </summary>
        public DocC(Expression content)
            : base(content)
        { }

        /// <summary>
        /// Parse a <see cref="DocC"/>.
        /// </summary>
        public DocC(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocC"/>.
        /// </summary>
        public static new DocC Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocC(parser, parent);
        }

        protected override bool ParseContent(Parser parser)
        {
            // Parse the content as code unless disabled or the content is empty
            if (DocCodeRefBase.ParseRefsAsCode && parser.Char != ParseTokenTagOpen[0])
            {
                // Override parsing of the content to parse as an expression
                Expression expression = parser.ParseCodeExpressionUntil(ParseTokenTagOpen + ParseTokenEndTag + ParseToken + ParseTokenTagClose, this);
                if (expression != null)
                    SetField(ref _content, expression, false);
                else
                    _content = "";

                // Look for expected end tag
                return ParseEndTag(parser);
            }
            return base.ParseContent(parser);
        }
    }
}