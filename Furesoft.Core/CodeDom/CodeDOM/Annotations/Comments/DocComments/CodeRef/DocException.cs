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
        #region /* CONSTRUCTORS */

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

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public override string TagName
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public new const string ParseToken = "exception";

        internal static void AddParsePoints()
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

        /// <summary>
        /// Parse a <see cref="DocException"/>.
        /// </summary>
        public DocException(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
