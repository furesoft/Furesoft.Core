// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Documents how to use a code object.
    /// </summary>
    public class DocExample : DocComment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocExample"/>.
        /// </summary>
        public DocExample(string text)
            : base(text)
        { }

        /// <summary>
        /// Create a <see cref="DocExample"/>.
        /// </summary>
        public DocExample(params DocComment[] docComments)
            : base(docComments)
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
        public new const string ParseToken = "example";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocExample"/>.
        /// </summary>
        public static new DocExample Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocExample(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocExample"/>.
        /// </summary>
        public DocExample(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        #endregion
    }
}
