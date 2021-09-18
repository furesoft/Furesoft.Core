﻿// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Documents the return value of a method.
    /// </summary>
    public class DocReturns : DocComment
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "returns";

        /// <summary>
        /// Create a <see cref="DocReturns"/>.
        /// </summary>
        public DocReturns(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocReturns"/>.
        /// </summary>
        public DocReturns(params DocComment[] docComments)
            : base(docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocReturns"/>.
        /// </summary>
        public DocReturns(Parser parser, CodeObject parent)
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

        /// <summary>
        /// Parse a <see cref="DocReturns"/>.
        /// </summary>
        public static new DocReturns Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocReturns(parser, parent);
        }

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }
    }
}