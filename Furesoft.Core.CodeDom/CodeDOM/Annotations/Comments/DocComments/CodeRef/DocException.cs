// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Documents that an exception can be thrown by a method, property, event, or indexer.
    /// </summary>
    public class DocException : DocCodeRefBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "exception";

        /// <summary>
        /// Create a <see cref="DocException"/>.
        /// </summary>
        public DocException(Expression codeRef, string text)
            : base(codeRef, text)
        { }

        /// <summary>
        /// Create a <see cref="DocException"/>.
        /// </summary>
        public DocException(Expression codeRef, params DocComment[] docComments)
            : base(codeRef, docComments)
        { }

        /// <summary>
        /// Parse a <see cref="DocException"/>.
        /// </summary>
        public DocException(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

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
        /// Parse a <see cref="DocException"/>.
        /// </summary>
        public static new DocException Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocException(parser, parent);
        }
    }
}