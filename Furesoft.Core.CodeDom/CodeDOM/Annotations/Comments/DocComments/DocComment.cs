// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Nova.Parsing;
using Nova.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents user documentation of code, and is also the common base class of <see cref="DocText"/>,
    /// <see cref="DocCodeRefBase"/>, <see cref="DocNameBase"/>, <see cref="DocB"/>, <see cref="DocC"/>, <see cref="DocCode"/>,
    /// <see cref="DocCDATA"/>, <see cref="DocExample"/>, <see cref="DocI"/>, <see cref="DocInclude"/>, <see cref="DocPara"/>,
    /// <see cref="DocRemarks"/>, <see cref="DocSummary"/>, <see cref="DocTag"/>, <see cref="DocValue"/>, <see cref="DocList"/>,
    /// <see cref="DocListHeader"/>, <see cref="DocListItem"/>, <see cref="DocListDescription"/>, <see cref="DocListTerm"/>.
    /// </summary>
    /// <remarks>
    /// This is the common base class of all documentation comment classes, but it can also be instantiated as
    /// a container of other documentation comment objects in cases where more than one is attached to the same
    /// code object.  For example, a DocSummary object can be attached to a code object by itself, or a DocComment
    /// can be attached which in turn can contain instances of DocSummary, DocParam, DocReturns, etc.
    /// C# uses an XML format for documentation comments, but Nova parses them and stores them as nested collections
    /// of code objects to make their manipulation and display easier.  References to code objects (DocParam, DocSee,
    /// etc) are stored as SymbolicRefs, and code embedded inside comments (DocCode, DocC) are stored as nested sub-
    /// trees of actual code objects, allowing for navigation and refactoring.
    /// Escape sequences in the XML for '&lt;', '&gt;', '&amp;' and '{}' for generics are handled during parsing.
    /// They are displayed normally in the GUI, but are allowed during editing, and are emitted in the encoded form
    /// for text output, thus being legal XML (although the C# compiler can handle the normal forms, also, and so
    /// does the Nova parser - but VS and Resharper have minor issues with them).
    /// </remarks>
    public class DocComment : CommentBase
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "///";

        /// <summary>
        /// The token used to assign values to attributes in document comments.
        /// </summary>
        public const string ParseTokenAssignAttrValue = "=";

        /// <summary>
        /// The start token for block-style document comments.
        /// </summary>
        public const string ParseTokenBlock = "/**";

        /// <summary>
        /// The token that indicates the end of a documentation comment XML tag.
        /// </summary>
        public const string ParseTokenEndTag = "/";

        /// <summary>
        /// The end token for documentation comment XML tags.
        /// </summary>
        public const string ParseTokenTagClose = ">";

        /// <summary>
        /// The start token for documentation comment XML tags.
        /// </summary>
        public const string ParseTokenTagOpen = "<";

        /// <summary>
        /// A token used to quote data in document comments.
        /// </summary>
        public const string ParseTokenValueQuote1 = "\"";

        /// <summary>
        /// A token used to quote data in document comments.
        /// </summary>
        public const string ParseTokenValueQuote2 = "'";

        /// <summary>
        /// The content can be a simple string or a ChildList of DocComment objects, or in some cases it can
        /// also be a sub-tree of embedded code objects.
        /// </summary>
        protected object _content;

        /// <summary>
        /// Create a <see cref="DocComment"/>.
        /// </summary>
        public DocComment()
        { }

        /// <summary>
        /// Create a <see cref="DocComment"/> with the specified text content.
        /// </summary>
        public DocComment(string text)
        {
            _content = (text != null ? text.Replace("\r\n", "\n") : null);  // Normalize newlines
        }

        /// <summary>
        /// Create a <see cref="DocComment"/> with the specified child <see cref="DocComment"/> content.
        /// </summary>
        public DocComment(DocComment docComment)
        {
            _content = new ChildList<DocComment>(this) { docComment };
        }

        /// <summary>
        /// Create a <see cref="DocComment"/> with the specified children <see cref="DocComment"/>s as content.
        /// </summary>
        public DocComment(params DocComment[] docComments)
        {
            Add(docComments);
        }

        /// <summary>
        /// Parse a <see cref="DocComment"/>.
        /// </summary>
        public DocComment(Parser parser, CodeObject parent)
        {
            Parent = parent;
            SetLineCol(parser.Token);
            ParseContent(parser);
        }

        /// <summary>
        /// Create a <see cref="DocComment"/> with the specified child <see cref="CodeObject"/> content.
        /// </summary>
        protected DocComment(CodeObject codeObject)
        {
            Content = codeObject;
        }

        /// <summary>
        /// The content of the documentation comment - can be a simple string, a ChildList of DocComment objects, or
        /// a sub-tree of embedded code objects.
        /// </summary>
        public object Content
        {
            get { return _content; }
            set { SetField(ref _content, value, true); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                if (base.IsSingleLine)
                {
                    if (_content == null)
                        return true;
                    if (_content is string)
                        return (((string)_content).IndexOf('\n') < 0);
                    if (_content is ChildList<DocComment>)
                        return !((ChildList<DocComment>)_content)[0].IsFirstOnLine && ((ChildList<DocComment>)_content).IsSingleLine;
                    if (_content is CodeObject)
                        return !((CodeObject)_content).IsFirstOnLine && ((CodeObject)_content).IsSingleLine;
                }
                return false;
            }
            set
            {
                base.IsSingleLine = value;
                if (_content is string)
                {
                    if (value)
                        _content = ((string)_content).Trim().Replace("\n", "; ");
                }
                else if (_content is ChildList<DocComment>)
                {
                    ChildList<DocComment> childList = (ChildList<DocComment>)_content;
                    if (value && childList.Count > 0)
                        childList[0].IsFirstOnLine = false;
                    childList.IsSingleLine = value;
                }
                else if (_content is CodeObject)
                {
                    if (value)
                        ((CodeObject)_content).IsFirstOnLine = false;
                    ((CodeObject)_content).IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// True if the documentation comment is missing an end tag.
        /// </summary>
        public bool MissingEndTag
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.NoEndTag); }
        }

        /// <summary>
        /// True if the documentation comment is missing a start tag.
        /// </summary>
        public bool MissingStartTag
        {
            get { return _annotationFlags.HasFlag(AnnotationFlags.NoStartTag); }
        }

        /// <summary>
        /// The XML tag name for the documentation comment.
        /// </summary>
        public virtual string TagName
        {
            get { return null; }
        }

        /// <summary>
        /// Implicit conversion of a <c>string</c> to a <see cref="DocComment"/> (actually, a <see cref="DocText"/>).
        /// </summary>
        /// <remarks>This allows strings to be passed directly to any method expecting a <see cref="DocComment"/> type
        /// without having to call <c>new DocText(text)</c>.</remarks>
        /// <param name="text">The <c>string</c> to be converted.</param>
        /// <returns>A generated <see cref="DocText"/> wrapping the specified <c>string</c>.</returns>
        public static implicit operator DocComment(string text)
        {
            return new DocText(text);
        }

        // NOTE: No parse-point is installed for general documentation comments - instead, the parser calls
        //       the parsing method below directly based upon the token type.  Documentation comments with
        //       specific tags do have parse-points installed.
        // NOTE: Manual parsing of the XML is done instead of using an XML parser - this is for
        //       performance, and to handle malformed XML properly, and also so embedded code references
        //       and fragments can be parsed properly with the main parser.
        /// <summary>
        /// Parse a <see cref="DocComment"/>.
        /// </summary>
        public static DocComment Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            Token token = parser.Token;
            byte prefixSpaceCount = (token.LeadingWhitespace.Length < byte.MaxValue ? (byte)token.LeadingWhitespace.Length : byte.MaxValue);

            // Get any newlines preceeding the documentation comment
            int newLines = token.NewLines;
            parser.NextToken(true);  // Move past '///' or '/**'

            // Start a new Unused list in the parser to catch unrecognized tokens in otherwise valid tags, etc.
            // This must be done in order to prevent anything already in the unused list from being emitted
            // within the doc comment.
            parser.PushUnusedList();

            // Remove any leading blank lines from inside the doc comment
            parser.Token.NewLines = 0;

            // Parse a DocComment object
            DocComment docComment = new DocComment(parser, parent) { NewLines = newLines };

            // Restore the previous Unused list in the parser - it's the responsibility of the DocComment parsing
            // logic to flush any unused tokens, such as into the content area of the comment.
            parser.PopUnusedList();

            // Remove the parent DocComment if it only has a single child
            if (docComment.Content is string)
            {
                DocText docText = new DocText((string)docComment.Content) { NewLines = newLines };
                docText.SetLineCol(docComment);
                docComment = docText;
            }
            else
            {
                ChildList<DocComment> content = (ChildList<DocComment>)docComment.Content;
                if (content.Count == 1)
                {
                    DocComment first = content[0];
                    first.NewLines = newLines;
                    first.SetLineCol(docComment);
                    docComment = first;
                }
            }

            // Store the number of prefixed spaces
            docComment._prefixSpaceCount = prefixSpaceCount;

            return docComment;
        }

        /// <summary>
        /// Add the specified text to the documentation comment.
        /// </summary>
        public virtual void Add(string text)
        {
            if (text != null)
            {
                if (_content == null)
                    _content = text;
                else if (_content is string)
                    _content += text;
                else if (_content is ChildList<DocComment>)
                {
                    ChildList<DocComment> children = (ChildList<DocComment>)_content;
                    if (children.Count == 0)
                        _content = text;
                    else if (children.Last is DocText)
                        children.Last.Add(text);
                    else
                        children.Add(new DocText(text));
                }
                else
                    throw new Exception("Can't add to a DocComment that contains code objects - add to the contained BlockDecl instead.");
            }
        }

        /// <summary>
        /// Add the specified child <see cref="DocComment"/> to the documentation comment.
        /// </summary>
        public virtual void Add(DocComment docComment)
        {
            if (docComment != null)
            {
                if (_content == null)
                    _content = new ChildList<DocComment>(this);
                else if (_content is string)
                {
                    string existing = (string)_content;
                    _content = new ChildList<DocComment>(this);
                    if (existing.Length > 0)  // Don't use NotEmpty(), because we want to preserve whitespace
                        ((ChildList<DocComment>)_content).Add(new DocText(existing));
                }
                if (docComment.GetType() == typeof(DocComment))
                {
                    // If we're adding a base container, merge the two containers instead
                    object content = docComment.Content;
                    if (content is string)
                        ((ChildList<DocComment>)_content).Add(new DocText((string)content));
                    else if (_content is ChildList<DocComment>)
                    {
                        ((ChildList<DocComment>)_content).AddRange((ChildList<DocComment>)content);
                        NormalizeContent();
                    }
                }
                else if (_content is ChildList<DocComment>)
                {
                    ((ChildList<DocComment>)_content).Add(docComment);
                    NormalizeContent();
                }
                else
                    throw new Exception("Can't add to a DocComment that contains code objects - add to the contained BlockDecl instead.");
            }
        }

        /// <summary>
        /// Add the specified <see cref="DocComment"/>s to the documentation comment.
        /// </summary>
        public void Add(params DocComment[] docComments)
        {
            foreach (DocComment docComment in docComments)
                Add(docComment);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
            int newLines = NewLines;
            bool isTopLevelDocComment = !flags.HasFlag(RenderFlags.InDocComment);
            if (isTopLevelDocComment)
            {
                if (!isPrefix && newLines > 0 && !flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
                AsTextDocNewLines(writer, 0);
            }
            else if (!isPrefix && newLines > 0)
                AsTextDocNewLines(writer, newLines);

            RenderFlags passFlags = (flags & RenderFlags.PassMask) | RenderFlags.InDocComment;
            UpdateLineCol(writer, flags);
            AsTextStart(writer, passFlags);
            AsTextContent(writer, passFlags);
            AsTextEnd(writer, passFlags);

            if (isTopLevelDocComment && isPrefix)
            {
                // If this object is rendered as a child prefix object of another, then any whitespace is
                // rendered here *after* the object instead of before it.
                // A documentation comment must always be followed by a newline if it's a prefix.
                writer.WriteLines(newLines < 1 ? 1 : newLines);
            }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            DocComment clone = (DocComment)base.Clone();
            if (_content is ChildList<DocComment>)
                clone._content = ChildListHelpers.Clone((ChildList<DocComment>)_content, clone);
            else
                clone.CloneField(ref clone._content, _content);
            return clone;
        }

        /// <summary>
        /// Returns the <see cref="DocSummary"/> documentation comment, or null if none exists.
        /// </summary>
        public override DocSummary GetDocSummary()
        {
            if (_content is ChildList<DocComment>)
                return Enumerable.FirstOrDefault(Enumerable.OfType<DocSummary>(((ChildList<DocComment>)_content)));
            return null;
        }

        /// <summary>
        /// Get the root documentation comment object.
        /// </summary>
        public DocComment GetRootDocComment()
        {
            DocComment parent = this;
            while (parent.Parent is DocComment)
                parent = (DocComment)parent.Parent;
            return parent;
        }

        /// <summary>
        /// Normalize content.
        /// </summary>
        public void NormalizeContent()
        {
            if (_content is ChildList<DocComment>)
            {
                ChildList<DocComment> children = (ChildList<DocComment>)_content;

                // Replace an empty collection with null
                if (children.Count == 0)
                    _content = null;
                else
                {
                    for (int i = children.Count - 1; i > 0; --i)
                    {
                        // Combine adjacent DocText objects into a single object
                        if (children[i] is DocText && children[i - 1] is DocText)
                        {
                            children[i - 1].Add(children[i].Text);
                            children.RemoveAt(i);
                        }
                    }
                    if (children.Count == 1)
                    {
                        CodeObject child = children[0];

                        // Replace a single DocText with a string
                        if (child is DocText)
                            _content = ((DocText)child).Text;
                        else if (child.NewLines > 0)
                        {
                            // Remove any newlines on the first child if they weren't explicitly set
                            if (!child.IsNewLinesSet && child.NewLines > 0)
                            {
                                // Move the newlines to the parent if it hasn't been explicitly set
                                if (!IsNewLinesSet)
                                    SetNewLines(child.NewLines);
                                child.SetNewLines(0);
                            }
                        }
                    }
                }
            }
        }

        protected internal string GetContentForDisplay(RenderFlags flags)
        {
            // If NoTagNewLines is set, trim any leading AND/OR trailing whitespace from the content (any newlines
            // determine if the content starts and/or ends on the same line as the start/end tag, and NoTagNewLines
            // means we don't want to render them because we're not rendering the tags).
            string content = (string)_content;
            if (flags.HasFlag(RenderFlags.NoTagNewLines))
                content = content.Trim();
            return content;
        }

        protected internal string GetContentForDisplay(DocText docText, bool isFirst, bool isLast, RenderFlags flags)
        {
            // If NoTagNewLines is set, trim any leading whitespace from the first child if it's a DocText, and trim
            // any trailing whitespace from the last child if it's a DocText.
            string text = docText.Text;
            if (flags.HasFlag(RenderFlags.NoTagNewLines))
            {
                if (isFirst)
                    text = text.TrimStart();
                else if (isLast)
                    text = text.TrimEnd();
            }
            return text;
        }

        protected static void AsTextDocNewLines(CodeWriter writer, int count)
        {
            // Render one or more newlines (0 means a prefix w/o a newline)
            do
            {
                if (count > 0)
                    writer.WriteLine();
                writer.Write(ParseToken + " ");
                --count;
            }
            while (count > 0);
        }

        protected virtual void AsTextContent(CodeWriter writer, RenderFlags flags)
        {
            writer.EscapeUnicode = false;
            if (_content is string)
                DocText.AsTextText(writer, GetContentForDisplay(flags), flags);
            else if (_content is ChildList<DocComment>)
            {
                ChildList<DocComment> content = (ChildList<DocComment>)_content;
                for (int i = 0; i < content.Count; ++i)
                {
                    DocComment docComment = content[i];
                    if (docComment is DocText)
                        DocText.AsTextText(writer, GetContentForDisplay((DocText)docComment, i == 0, i == content.Count - 1, flags), flags);
                    else
                        docComment.AsText(writer, flags);
                }
            }
            else if (_content is CodeObject)
            {
                // Turn on translation of '<', '&', and '>' for content
                writer.InDocCommentContent = true;
                ((CodeObject)_content).AsText(writer, flags);
                writer.InDocCommentContent = false;
            }
            writer.EscapeUnicode = true;
        }

        protected virtual void AsTextEnd(CodeWriter writer, RenderFlags flags)
        {
            if (!MissingEndTag && (_content != null || MissingStartTag) && !flags.HasFlag(RenderFlags.Description))
            {
                string tagName = TagName;
                if (tagName != null)
                    writer.Write("</" + tagName + ">");
            }
        }

        protected virtual void AsTextStart(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description) || MissingEndTag)
            {
                string tagName = TagName;
                if (tagName != null)
                    writer.Write("<" + tagName + (_content == null && !MissingEndTag ? "/>" : ">"));
            }
        }

        protected virtual object ParseAttributeValue(Parser parser, string name)
        {
            // By default, parse a string value (including any whitespace) delimited by single or double quotes.
            // If there's no delimiter, just use the text of the token (perhaps a single word).
            string value;
            if (parser.TokenText == ParseTokenValueQuote1 || parser.TokenText == ParseTokenValueQuote2)
                value = parser.GetToDelimiter(parser.TokenText[0]);
            else
                value = parser.TokenText;
            parser.NextToken(true);  // Move past delimiter (or token)
            return value;
        }

        /// <summary>
        /// Parse the content of a <see cref="DocComment"/> tag.
        /// </summary>
        /// <returns>True if the content was followed by a valid end tag, otherwise false.</returns>
        protected virtual bool ParseContent(Parser parser)
        {
            bool foundEndTag = false;

            // Default to an empty string (leaving it null would combine the end tag with the start tag)
            _content = "";

            if (parser.TokenText == ParseTokenTagClose)
                parser.NextToken(true);  // Move past '>'

            // Special check for a comment terminating a doc comment
            Token lastToken = parser.LastToken;
            if (lastToken != null && lastToken.Text == null && lastToken.HasTrailingComments)
            {
                foreach (CommentBase commentBase in parser.LastToken.TrailingComments)
                    _content += (commentBase.IsFirstOnLine ? "\n" : "") + commentBase.AsString();
                parser.LastToken.TrailingComments = null;
            }

            // Stop if we hit EOF, or if we've exited the doc comment and processed the last doc comment string
            while (parser.Token != null && (parser.InDocComment || parser.TokenType == TokenType.DocCommentString))
            {
                DocComment comment = null;

                // Look for any embedded start/end tag
                if (parser.TokenText == ParseTokenTagOpen && !parser.Token.WasEscaped)
                {
                    // Peek ahead first to determine if the end tag matches a parent's open
                    // tag instead of the current one.
                    if (parser.PeekNextTokenText() == ParseTokenEndTag)
                    {
                        string endTagName = parser.PeekNextTokenText();
                        if (endTagName != TagName)
                        {
                            // If the end tag doesn't match the current open tag, but does
                            // match a parent's open tag, then abort processing this tag.
                            DocComment parent = _parent as DocComment;
                            while (parent != null)
                            {
                                if (endTagName == parent.TagName)
                                    break;
                                parent = parent.Parent as DocComment;
                            }
                            if (parent != null)
                            {
                                parser.ResetPeekAhead();
                                break;
                            }
                        }
                    }

                    // Add any leading whitespace on the tag as text
                    if (parser.Token.LeadingWhitespace.Length > 0)
                    {
                        // If the token is on a new line, insert a newline in the text, and change the token to NOT be on a new line.
                        string whitespace = parser.Token.LeadingWhitespace;
                        if (parser.Token.IsFirstOnLine)
                        {
                            whitespace = '\n' + whitespace;
                            parser.Token.NewLines = 0;
                        }
                        Add(whitespace);
                    }

                    Token openTagToken = parser.Token;
                    parser.NextToken(true);  // Move past '<'

                    if (parser.TokenText == ParseTokenEndTag && !parser.Token.WasEscaped)
                    {
                        int newLines = parser.LastToken.NewLines;

                        // Handle an end tag
                        parser.NextToken(true);  // Move past '/'
                        Token endTag = parser.Token;
                        parser.NextToken(true);  // Move past tag
                        if (parser.TokenText == ParseTokenTagClose)
                            parser.NextToken(true);  // Move past '>'

                        // If the end tag matches the current open tag, we're done
                        if (endTag.Text == TagName)
                        {
                            // Add any newlines on the end tag as text content
                            if (newLines > 0)
                                Add(new string('\n', newLines));
                            foundEndTag = true;
                            break;
                        }

                        // Handle an unexpected end tag
                        comment = new DocTag(endTag, newLines, parser, this);
                    }
                    else
                    {
                        // Recursively parse a start tag
                        comment = (DocComment)parser.ProcessToken(this);
                    }
                    if (comment != null)
                        Add(comment);
                    else
                    {
                        // If we failed to parse a tag, save the open tag and last unused tokens for parsing
                        // into comment text below.
                        Token lastUnusedToken = parser.RemoveLastUnused().AsToken();
                        parser.AddUnused(openTagToken);
                        parser.AddUnused(lastUnusedToken);
                    }
                }

                // If we didn't parse a tag, then handle comment text
                if (comment == null)
                {
                    string text;
                    if (parser.Token.TokenType != TokenType.DocCommentStart)
                    {
                        // Handle comment text
                        text = parser.Token.LeadingWhitespace + parser.TokenText;

                        // Add any newlines to the front of the text
                        if (parser.Token.NewLines > 0)
                            text = new string('\n', parser.Token.NewLines) + text;
                    }
                    else
                        text = "\n";

                    // Flush any unused tokens to the front of the text
                    while (parser.HasUnused)
                    {
                        Token unusedToken = parser.RemoveLastUnused().AsToken();
                        text = unusedToken.LeadingWhitespace + unusedToken.Text + text;
                    }

                    parser.NextToken(true);  // Move past text

                    // If we're at the end of the doc comment, truncate the trailing newline
                    if (!(parser.InDocComment || parser.TokenType == TokenType.DocCommentString))
                        text = text.TrimEnd('\n');
                    Add(text);
                }
            }

            return foundEndTag;
        }

        protected bool ParseEndTag(Parser parser)
        {
            // Look for expected end tag
            if (parser.TokenText == ParseTokenTagOpen && !parser.Token.WasEscaped)
            {
                Token next1 = parser.PeekNextToken();
                if (next1 != null && next1.Text == ParseTokenEndTag)
                {
                    Token next2 = parser.PeekNextToken();
                    if (next2 != null && next2.Text == TagName)
                    {
                        parser.NextToken(true);  // Move past '<'

                        // Add any newlines on the end tag as text content, but ignore if the
                        // content is code objects (we're a DocCode or DocC).
                        if (parser.LastToken.NewLines > 0 && !(_content is CodeObject))
                            Add(new string('\n', parser.LastToken.NewLines));

                        parser.NextToken(true);  // Move past '/'
                        parser.NextToken(true);  // Move past tag name
                        if (parser.TokenText == ParseTokenTagClose)
                            parser.NextToken(true);  // Move past '>'

                        return true;
                    }
                }
            }
            return false;
        }

        protected Dictionary<string, object> ParseTag(Parser parser, CodeObject parent)
        {
            Parent = parent;
            Token lastToken = parser.LastToken;
            NewLines = lastToken.NewLines;  // Get any newlines from the '<'
            SetLineCol(lastToken);
            Token tagToken = parser.Token;
            parser.NextToken(true);  // Move past tag

            Dictionary<string, object> attributes = ParseAttributes(parser);
            bool endTag = (parser.TokenText == ParseTokenEndTag);
            if (endTag)
                parser.NextToken(true);  // Move past '/'
            if (parser.TokenText == ParseTokenTagClose)
            {
                if (endTag)
                    parser.NextToken(true);  // Move past '>'
                else
                {
                    if (!ParseContent(parser))
                    {
                        _annotationFlags |= AnnotationFlags.NoEndTag;
                        parser.AttachMessage(this, "Start tag '<" + TagName + (attributes == null ? '>' : ' ') + "' without matching end tag!", tagToken);
                    }
                }
            }
            else
                parser.AttachMessage(this, endTag ? "'>' expected" : "'>' or '/>' expected", tagToken);
            return attributes;
        }

        private Dictionary<string, object> ParseAttributes(Parser parser)
        {
            Dictionary<string, object> attributes = null;

            // Stop looping if we hit the end of the file or the end of the open tag, or an unexpected new open tag
            while (parser.Token != null && parser.InDocComment && !((parser.TokenText == ParseTokenEndTag
                || parser.TokenText == ParseTokenTagClose || parser.TokenText == ParseTokenTagOpen) && !parser.Token.WasEscaped))
            {
                if (parser.Token.IsDocCommentTag)
                {
                    string name = parser.TokenText;
                    parser.NextToken(true);  // Move past name
                    if (parser.TokenText == ParseTokenAssignAttrValue)
                    {
                        parser.NextToken(true);  // Move past '='
                        object value = ParseAttributeValue(parser, name);
                        if (attributes == null)
                            attributes = new Dictionary<string, object>();
                        attributes.Add(name, value);
                    }
                }
                else
                {
                    parser.AttachMessage(this, "'" + parser.Token + "' unrecognized - ignored", parser.Token);
                    parser.NextToken(true);  // Move past unexpected token
                }
            }
            return attributes;
        }
    }
}