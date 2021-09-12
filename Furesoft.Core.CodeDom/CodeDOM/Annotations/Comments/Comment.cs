// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Linq;

using Nova.Parsing;
using Nova.Rendering;
using Nova.Utilities;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents user comments, and may either be independent or associated with another <see cref="CodeObject"/>.
    /// </summary>
    /// <remarks>
    /// A comment can be flagged as EOL, Block, TODO, HACK, NOTE, or combinations of these.
    /// By default, comments use the '//' style, unless flagged as Block, in which case they
    /// use the '/*...*/' style.
    /// Regular comments appear before the object they belong to, and are usually on separate
    /// lines, although they can sometimes be on the same line, in which case they are forcibly
    /// rendered in the Block style whether flagged as such or not.
    /// Comments flagged as EOL appear after the object on the same line, and are usually the
    /// last thing on the line, although they can sometimes be followed by other objects, in
    /// which case they are forcibly rendered in the Block style whether flagged as such or not.
    /// Comments can also have a "Post" format, which means they appear after the object.  EOL
    /// comments are usually Post, but can be non-post in some cases, such as when they appear
    /// after the opening symbol ('{') of a Block.
    /// A code object can have a regular comment, an EOL comment, or both.  Technically,
    /// multiple comments of each type are supported, but having more than one of the same type,
    /// or a regular comment in addition to a documentation comment, is very rare and not
    /// recommended - it gets messy in the UI, and helper properties are provided for each
    /// comment type which only return the first comment of that type found.
    /// A regular comment can have multiple lines of text whether Block type or not, but EOL
    /// comments shouldn't - newlines will be converted to ';' on display if present.
    /// </remarks>
    public class Comment : CommentBase
    {
        #region /* STATIC FIELDS */

        /// <summary>
        /// Determines if special comments are listed.
        /// </summary>
        public static bool ListSpecialComments = true;

        #endregion

        #region /* FIELDS */

        protected string _text;
        protected CommentFlags _commentFlags;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Comment"/> with the specified text content.
        /// </summary>
        public Comment(string text, CommentFlags flags)
        {
            Text = text;
            _commentFlags = flags;

            // Default to same line for EOL comments, and separate line for regular comments
            SetNewLines(flags.HasFlag(CommentFlags.EOL) ? 0 : 1);
        }

        /// <summary>
        /// Create a <see cref="Comment"/> with the specified text content.
        /// </summary>
        public Comment(string text)
            : this(text, CommentFlags.None)
        { }

        #endregion

        #region /* STATIC CONSTRUCTOR */

        static Comment()
        {
            // Force a reference to CodeObject to trigger the loading of any config file if it hasn't been done yet
            ForceReference();
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The text content of the comment.
        /// </summary>
        public override string Text
        {
            get { return _text; }
            set
            {
                _text = value.Replace("\r\n", "\n");  // Normalize newlines
                SetSpecialFlags();
            }
        }

        /// <summary>
        /// True if the comment appears at the end-of-line (EOL).
        /// </summary>
        public override bool IsEOL
        {
            get { return _commentFlags.HasFlag(CommentFlags.EOL); }
            set { SetCommentFlag(CommentFlags.EOL, value); }
        }

        /// <summary>
        /// Determines if the comment has a block style.
        /// </summary>
        public bool IsBlock
        {
            get { return _commentFlags.HasFlag(CommentFlags.Block); }
            set { SetCommentFlag(CommentFlags.Block, value); }
        }

        /// <summary>
        /// Determines if the comment has a raw format.
        /// </summary>
        public bool IsRawFormat
        {
            get { return _commentFlags.HasFlag(CommentFlags.Raw); }
            set { SetCommentFlag(CommentFlags.Raw, value); }
        }

        /// <summary>
        /// Determines if the comment is a 'TODO' comment.
        /// </summary>
        public bool IsTODO
        {
            get { return _commentFlags.HasFlag(CommentFlags.TODO); }
            set { SetCommentFlag(CommentFlags.TODO, value); }
        }

        /// <summary>
        /// Determines if the comment is a 'HACK' comment.
        /// </summary>
        public bool IsHack
        {
            get { return _commentFlags.HasFlag(CommentFlags.HACK); }
            set { SetCommentFlag(CommentFlags.HACK, value); }
        }

        /// <summary>
        /// Determines if the comment is a 'NOTE' comment.
        /// </summary>
        public bool IsNote
        {
            get { return _commentFlags.HasFlag(CommentFlags.NOTE); }
            set { SetCommentFlag(CommentFlags.NOTE, value); }
        }

        /// <summary>
        /// True if the annotation should be listed at the <see cref="CodeUnit"/> and <see cref="Solution"/> levels (for display in an output window).
        /// </summary>
        public override bool IsListed
        {
            get { return (ListSpecialComments && (_commentFlags.HasFlag(CommentFlags.TODO) || _commentFlags.HasFlag(CommentFlags.HACK))); }
        }

        /// <summary>
        /// The comment flags.
        /// </summary>
        public CommentFlags CommentFlags
        {
            get { return _commentFlags; }
        }

        #endregion

        #region /* METHODS */

        protected internal void SetSpecialFlags()
        {
            _commentFlags |= CheckForSpecialComment(_text);
        }

        /// <summary>
        /// Check if text represents a "special" comment.
        /// </summary>
        /// <returns>CommentFlag for special comment type, or None.</returns>
        protected internal CommentFlags CheckForSpecialComment(string comment)
        {
            if (ContainsSpecial(comment, "TODO"))
                return CommentFlags.TODO;
            if (ContainsSpecial(comment, "HACK"))
                return CommentFlags.HACK;
            if (ContainsSpecial(comment, "NOTE"))
                return CommentFlags.NOTE;
            return CommentFlags.None;
        }

        protected bool ContainsSpecial(string text, string special)
        {
            // Match special comment indicators by finding a case-insensitive match
            int index = text.IndexOf(special, StringComparison.CurrentCultureIgnoreCase);
            if (index >= 0)
            {
                if (index >= 1)
                {
                    // Ignore if there is an immediately preceeding alpha char, or a '/' (nested in another comment) or '"' ignoring spaces
                    char preceeding = text[index - 1];
                    if (char.IsLetter(preceeding))
                        return false;
                    int preIndex = index - 1;
                    while (preceeding == ' ' && preIndex > 0)
                        preceeding = text[--preIndex];
                    if (preceeding == '/' || preceeding == '"')
                        return false;
                }
                // Ignore if there is a trailing alpha char
                int end = index + special.Length;
                if (end < text.Length && char.IsLetter(text, end))
                    return false;
                return true;
            }
            return false;
        }

        protected internal void SetCommentFlag(CommentFlags flag, bool value)
        {
            if (value)
                _commentFlags |= flag;
            else
                _commentFlags &= ~flag;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "//";

        /// <summary>
        /// The block-comment start token.
        /// </summary>
        public const string ParseTokenBlockStart = "/*";

        /// <summary>
        /// The block-comment end token.
        /// </summary>
        public const string ParseTokenBlockEnd = "*/";

        // NOTE: No parse-points are installed for comments - instead, the parser calls the
        //       parsing constructor directly based upon the token type.  This is because we
        //       want to treat entire comments as individual tokens to preserve whitespace.

        /// <summary>
        /// Parse a <see cref="Comment"/>.
        /// </summary>
        public Comment(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            Token token = parser.Token;
            _prefixSpaceCount = (token.LeadingWhitespace.Length < byte.MaxValue ? (byte)token.LeadingWhitespace.Length : byte.MaxValue);
            string text = token.Text;
            int start;
            bool noSpaceAfterDelimiter;

            // Parse a standard '//' comment
            if (text.StartsWith(ParseToken))
            {
                // Keep track of whether or not all '//' are followed by a space
                start = ParseToken.Length;
                noSpaceAfterDelimiter = (start < text.Length && text[start] != ' ');
                _text = text.Substring(start).TrimEnd();

                if (!IsFirstOnLine)
                {
                    // The comment occurred after another token on the same line - it's an EOL comment
                    _commentFlags |= CommentFlags.EOL;
                    parser.NextToken(true);
                }
                else
                {
                    // The comment was on a line by itself - combine with following lines if they're also comments,
                    // and they're compatible - not Doc or block comments, same # of prefix spaces, no special prefix.
                    while (true)
                    {
                        int nextLine = token.LineNumber + 1;
                        token = parser.NextToken(true);
                        if ((token != null) && token.IsComment && token.Text.StartsWith(ParseToken)
                            && (token.LineNumber == nextLine) && (token.LeadingWhitespace.Length == _prefixSpaceCount)
                            && (string.IsNullOrEmpty(_text) || CheckForSpecialComment(token.Text.Substring(ParseToken.Length)) == CommentFlags.None))
                        {
                            text = parser.TokenText;
                            start = ParseToken.Length;
                            if (start < text.Length && text[start] != ' ')
                                noSpaceAfterDelimiter = true;
                            _text += "\n" + text.Substring(start).TrimEnd();
                        }
                        else
                            break;
                    }

                    // If the comment was left-justified, set the format flag as such so that it will be displayed
                    // at the left margin regardless of the current level of code indentation.
                    if (_prefixSpaceCount == 0)
                        _formatFlags |= FormatFlags.NoIndentation;
                }
            }
            else
            {
                // Parse a '/* ... */' block comment
                IsBlock = true;
                noSpaceAfterDelimiter = false;
                bool formatted = true;

                // The parser will pass through everything between the delimiters, so we have to deal with
                // that here by breaking it up into lines, stripping leading spaces and asterisks, and then
                // putting them back together.
                string[] lines = text.Split('\n');
                for (int i = 0; i < lines.Length; ++i)
                {
                    string line = lines[i];
                    bool hasBlockEnd = (i == lines.Length - 1 && line.EndsWith(ParseTokenBlockEnd));
                    if (i == 0)
                        start = ParseTokenBlockStart.Length;
                    else
                    {
                        // Skip leading spaces on subsequent lines up to the indent of the first line
                        start = Math.Min(_prefixSpaceCount, StringUtil.CharCount(line, ' ', 0));

                        // Special format handling: Skip a leading "*"
                        if (start < line.Length && line[start] == '*' && !(hasBlockEnd && start == line.Length - 2))
                            ++start;
                        else if (start < line.Length - 1)
                        {
                            // Special format handling: Skip " *" or "\" if followed by '*' and on the last line (if not part of the ending "*/"),
                            // or "//" (and NOT "///"), or "  " (if followed by a non-space).
                            if ((line[start] == ' ' && line[start + 1] == '*') || (line[start] == '\\' && line[start + 1] == '*' && i == lines.Length - 1))
                            {
                                if (!(hasBlockEnd && start == line.Length - 3))
                                    start += 2;
                            }
                            else if ((line[start] == '/' && line[start + 1] == '/' && (start == line.Length - 2 || line[start + 2] != '/'))
                                || (line[start] == ' ' && line[start + 1] == ' ' && (start == line.Length - 2 || line[start + 2] != ' ')))
                            {
                                start += 2;
                                formatted = false;
                            }
                            else
                                formatted = false;
                        }
                    }
                    if (start < line.Length && line[start] != ' ')
                        noSpaceAfterDelimiter = true;
                    int length = line.Length - start;
                    if (hasBlockEnd)
                        length -= ParseTokenBlockEnd.Length;
                    _text += (i == 0 ? "" : "\n") + line.Substring(start, length).TrimEnd();
                }

                if (!formatted)
                    _commentFlags |= CommentFlags.Raw;

                Token nextToken = parser.NextToken(true);

                // If the comment occurred after another token on the same line AND it's a single line block
                // comment AND it's the last token on the line - it's an inline EOL comment.
                if (!IsFirstOnLine && lines.Length == 1 && nextToken.IsFirstOnLine)
                    _commentFlags |= CommentFlags.EOL;
            }

            // If any line is missing a space after the delimiter, then set the special flag indicating that no
            // spaces should be displayed.  Otherwise, remove one column of spaces and add it back when displayed.
            if (noSpaceAfterDelimiter)
                NoSpaceAfterDelimiter = true;
            else
                RemoveSpaces(1);

            SetSpecialFlags();
        }

        /// <summary>
        /// Remove the specified number of prefixed spaces from each line of the comment.
        /// Fails if any non-blank lines don't start with at least the specified number of spaces.
        /// </summary>
        public bool RemoveSpaces(int spaces)
        {
            string[] lines = _text.Split('\n');
            if (Enumerable.Any(lines, delegate(string line) { return StringUtil.CharCount(line, ' ', 0) < spaces && line.Length > 0; }))
                return false;
            _text = null;
            foreach (string line in lines)
                _text += (_text == null ? "" : "\n") + (string.IsNullOrEmpty(line) ? "" : line.Remove(0, spaces));
            return true;
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if there is no space between the comment delimiter and the comment text.
        /// </summary>
        public bool NoSpaceAfterDelimiter
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.NoSpace); }
            set { SetAnnotationFlag(AnnotationFlags.NoSpace, value); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (IsEOL || (_text == null || _text.IndexOf('\n') < 0))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _text != null)
                    _text = _text.Trim().Replace("\n", "; ");
            }
        }

        #endregion

        #region /* RENDERING */

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            bool isEOL = IsEOL;
            bool isInline = flags.HasFlag(RenderFlags.CommentsInline);
            bool isBlock = (IsBlock || isInline);
            bool isRawFormat = IsRawFormat;

            int newLines = NewLines;
            bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
            if (!isPrefix)
            {
                if (newLines > 0)
                {
                    if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                        writer.WriteLines(newLines);
                }
                else if (!IsInfix || isEOL)
                {
                    // Handle a special case of an EOL comment on an 'else' that is actually commented-out
                    // code for an 'if' (common practice to document an implied 'if' condition).
                    bool elseIf = (isEOL && Parent is Else && _text.StartsWith("if ("));
                    writer.Write(isBlock || elseIf ? " " : "  ");
                }
            }

            if (_text == null)
                UpdateLineCol(writer, flags);
            else
            {
                string space = (NoSpaceAfterDelimiter ? "" : " ");
                writer.EscapeUnicode = false;
                if (isEOL || isInline)
                {
                    // Render a single-line EOL or in-line block comment.
                    // There shouldn't be any newlines in the comment, but handle them just in case.
                    UpdateLineCol(writer, flags);
                    string comment = (isBlock ? ParseTokenBlockStart : ParseToken) + space
                        + _text.Replace("\n", "; ") + (isBlock ? ((_text.EndsWith("*") ? "" : space) + ParseTokenBlockEnd) : "");
                    writer.Write(comment);
                }
                else
                {
                    // Render a non-EOL (one or more lines) comment or block comment
                    if (HasNoIndentation)
                        writer.BeginOutdentOnNewLine(this, 0);
                    UpdateLineCol(writer, flags);
                    string[] lines = _text.Split('\n');
                    bool lastIsEmpty = true;
                    for (int i = 0; i < lines.Length; ++i)
                    {
                        string line = lines[i];
                        bool isEmptyLine = string.IsNullOrEmpty(line);
                        string prefixSpace = (isEmptyLine ? "" : space);
                        if (i > 0)
                            writer.WriteLine();

                        // Write comment prefix as appropriate
                        if (isBlock)
                        {
                            lastIsEmpty = ((i == lines.Length - 1) && isEmptyLine);
                            writer.Write(i == 0 ? ParseTokenBlockStart + prefixSpace : (isRawFormat ? "" : (lastIsEmpty ? " " : " *" + prefixSpace)));
                            if (lastIsEmpty)
                                break;
                        }
                        else
                            writer.Write(ParseToken + prefixSpace);

                        writer.Write(line);
                    }
                    if (isBlock)
                    {
                        space = ((isRawFormat || lastIsEmpty || _text.EndsWith("*")) ? "" : space);
                        writer.Write(space + ParseTokenBlockEnd);
                    }
                    if (HasNoIndentation)
                        writer.EndIndentation(this);
                }
                writer.EscapeUnicode = true;
            }

            if (isPrefix)
            {
                // If this object is rendered as a child prefix object of another, then any whitespace is
                // rendered here *after* the object instead of before it.
                if (newLines > 0)
                    writer.WriteLines(newLines);
                else
                    writer.Write(" ");
            }
        }

        /// <summary>
        /// Get any <see cref="CommentFlags"/> and <see cref="AnnotationFlags"/> as a comma separated string.
        /// </summary>
        public string FlagsAsText()
        {
            string result = "";
            if (_commentFlags != CommentFlags.None)
                result += _commentFlags;
            if (_annotationFlags != AnnotationFlags.None || string.IsNullOrEmpty(result))
                result = StringUtil.Append(result, ", ", _annotationFlags.ToString());
            return result;
        }

        #endregion
    }

    #region /* COMMENT TYPE FLAGS */

    /// <summary>
    /// Comment type flags.
    /// </summary>
    [Flags]
    public enum CommentFlags
    {
        /// <summary>No flags.</summary>
        None  = 0x00,
        /// <summary>The comment is an "end-of-line" comment - the last item on the current line.</summary>
        EOL   = 0x01,
        /// <summary>The comment is "block" style.</summary>
        Block = 0x02,
        /// <summary>The comment isn't formatted - for block style, this means no preceeding asterisks on each line.</summary>
        Raw   = 0x04,
        /// <summary>The comment is a 'to-do' comment.</summary>
        TODO  = 0x10,
        /// <summary>The comment documents a 'hack' in the code.</summary>
        HACK  = 0x20,
        /// <summary>The comment represents a special note.</summary>
        NOTE  = 0x40
    }

    #endregion
}
