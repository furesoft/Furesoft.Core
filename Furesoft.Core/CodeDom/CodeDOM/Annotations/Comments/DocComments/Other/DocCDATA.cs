// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a section of character data in a documentation comment.
    /// </summary>
    public class DocCDATA : DocComment
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="DocCDATA"/>.
        /// </summary>
        public DocCDATA(string content)
            : base(content)
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
        public new const string ParseToken = "![CDATA[";

        /// <summary>
        /// The closing token.
        /// </summary>
        public const string ParseTokenClose = "]]>";

        internal static void AddParsePoints()
        {
            Parser.AddDocCommentParseTag(ParseToken, Parse);
        }

        /// <summary>
        /// Parse a <see cref="DocCDATA"/>.
        /// </summary>
        public static new DocCDATA Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new DocCDATA(parser, parent);
        }

        /// <summary>
        /// Parse a <see cref="DocCDATA"/>.
        /// </summary>
        public DocCDATA(Parser parser, CodeObject parent)
        {
            Parent = parent;
            NewLines = parser.LastToken.NewLines;  // Get any newlines from the '<'
            parser.NextToken(true);                // Move past '![CDATA['
            if (!ParseContent(parser))
            {
                _annotationFlags |= AnnotationFlags.NoEndTag;
                parser.AttachMessage(this, "Start tag '<" + ParseToken + "' without matching end tag!", parser.LastToken);
            }
        }

        protected override bool ParseContent(Parser parser)
        {
            _content = "";

            // Skip if we hit EOF, or the close token, or we've exited the doc comment or got an invalid token type
            if (parser.Token != null && parser.TokenText != ParseTokenClose && (parser.InDocComment || parser.TokenType == TokenType.DocCommentString))
            {
                // Handle comment text
                string text = parser.Token.LeadingWhitespace + parser.TokenText;

                // Add any newlines to the front of the text
                if (parser.Token.NewLines > 0)
                    text = new string('\n', parser.Token.NewLines) + text;

                // If we're at the end of the doc comment, truncate the trailing newline
                if (!parser.InDocComment)
                    text = text.TrimEnd('\n');

                Add(text);
                parser.NextToken(true);  // Move past text
            }

            if (parser.TokenText == ParseTokenClose)
            {
                parser.NextToken(true);  // Move past ']]>'
                return true;
            }

            return false;
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

        #region /* RENDERING */

        protected override void AsTextStart(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description) || MissingEndTag)
                writer.Write("<" + ParseToken);
        }

        protected override void AsTextContent(CodeWriter writer, RenderFlags flags)
        {
            DocText.AsTextText(writer, GetContentForDisplay(flags), flags | RenderFlags.NoTranslations);
        }

        protected override void AsTextEnd(CodeWriter writer, RenderFlags flags)
        {
            if (!MissingEndTag && !flags.HasFlag(RenderFlags.Description))
                writer.Write(ParseTokenClose);
        }

        #endregion
    }
}
