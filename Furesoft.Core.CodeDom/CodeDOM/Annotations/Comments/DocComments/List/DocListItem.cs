// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents an item in a <see cref="DocList"/> in a documentation comment.
    /// </summary>
    public class DocListItem : DocComment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocListItem"/>.
        /// </summary>
        public DocListItem(params DocComment[] docComments)
        {
            foreach (DocComment docComment in docComments)
            {
                // Default-format entries
                Add("\n        ");
                Add(docComment);
            }

            // Default end tag to first-on-line
            Add("\n    ");
        }

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
        public new const string ParseToken = "item";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocListItem"/>.
        /// </summary>
        public static new DocListItem Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocListItem(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocListItem"/>.
        /// </summary>
        public DocListItem(Parser parser, CodeObject parent)
        {
            ParseTag(parser, parent);  // Ignore any attributes
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            get { return false; }
        }

        #endregion
    }
}
