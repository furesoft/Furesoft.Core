// The Furesoft.Core.CodeDom Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM
{
    /// <summary>
    /// Provides a summary comment for a code object.
    /// </summary>
    public class DocSummary : DocComment
    {
        /// <summary>
        /// Create a <see cref="DocSummary"/>.
        /// </summary>
        public DocSummary(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocSummary"/>.
        /// </summary>
        public DocSummary(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// Returns the <see cref="DocSummary"/> itself.
        /// </summary>
        public override DocSummary GetDocSummary()
        {
            return this;
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "summary";

        /// <summary>
        /// Parse a <see cref="DocSummary"/>.
        /// </summary>
        public DocSummary(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        /// <summary>
        /// Parse a <see cref="DocSummary"/>.
        /// </summary>
        public static new DocSummary Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocSummary(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}