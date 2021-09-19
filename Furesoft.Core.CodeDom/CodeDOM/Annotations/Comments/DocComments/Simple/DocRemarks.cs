﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Provides additional comments for a code object.
    /// </summary>
    public class DocRemarks : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "remarks";

        /// <summary>
        /// Create a <see cref="DocRemarks"/>.
        /// </summary>
        public DocRemarks(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocRemarks"/>.
        /// </summary>
        public DocRemarks(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocRemarks"/>.
        /// </summary>
        public DocRemarks(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
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
        /// Parse a <see cref="DocRemarks"/>.
        /// </summary>
        public static new DocRemarks Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocRemarks(parser, parent);
        }
    }
}