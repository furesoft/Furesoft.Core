// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Represents the term of a <see cref="DocListItem"/> in a <see cref="DocList"/> in a documentation comment.
    /// </summary>
    public class DocListTerm : DocComment
    {
        /// <summary>
        /// Create a <see cref="DocListTerm"/>.
        /// </summary>
        public DocListTerm(string content)
            : base(content)
        { }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "term";

        /// <summary>
        /// Parse a <see cref="DocListTerm"/>.
        /// </summary>
        public DocListTerm(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// Parse a <see cref="DocListTerm"/>.
        /// </summary>
        public static new DocListTerm Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocListTerm(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }
    }
}