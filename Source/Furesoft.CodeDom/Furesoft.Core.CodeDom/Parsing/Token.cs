// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System.Collections.Generic;
using Furesoft.Core.CodeDom.Parsing.Base;
using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;

namespace Furesoft.Core.CodeDom.Parsing;

/// <summary>
/// The general type of a <see cref="Token"/>.
/// </summary>
public enum TokenType : byte { None, Identifier, Symbol, VerbatimString, String, Char, Numeric, Comment,
    DocCommentStart, DocCommentTag, DocCommentSymbol, DocCommentString, CompilerDirective }

/// <summary>
/// Represents a discreet unit of text parsed from the input stream, along with location
/// and formatting information, and any trailing comments.
/// </summary>
public class Token : ParsedObject
{
    #region /* FIELDS */

    /// <summary>
    /// The text of the <see cref="Token"/>.
    /// </summary>
    public string Text;

    /// <summary>
    /// The <see cref="TokenType"/> of the <see cref="Token"/>.
    /// </summary>
    public TokenType TokenType;

    /// <summary>
    /// True if the <see cref="Token"/> was escaped.
    /// </summary>
    public bool WasEscaped;

    /// <summary>
    /// True if inside a documentation comment.
    /// </summary>
    protected bool _inDocComment;

    /// <summary>
    /// The line number of the <see cref="Token"/> (1 to N).
    /// </summary>
    public int LineNumber;

    /// <summary>
    /// The column number of the <see cref="Token"/> (1 to N).
    /// </summary>
    public ushort ColumnNumber;

    /// <summary>
    /// The number of new lines preceeding the <see cref="Token"/>.
    /// </summary>
    public ushort NewLines;

    /// <summary>
    /// Any leading whitespace on the <see cref="Token"/>.  Will be empty if none (not null).
    /// </summary>
    public string LeadingWhitespace;

    /// <summary>
    /// Any trailing comments on the <see cref="Token"/>.
    /// </summary>
    public List<CommentBase> TrailingComments;

#if DEBUG
    /// <summary>
    /// The parent <see cref="CodeUnit"/> (used in Debug mode to track lost comments).
    /// </summary>
    public CodeUnit CodeUnit;
#endif

    #endregion

    #region /* CONSTRUCTORS */

#if DEBUG
    /// <summary>
    /// Create a <see cref="Token"/>.
    /// </summary>
    public Token(string text, TokenType tokenType, bool wasEscaped, bool inDocComment, int lineNumber, int columnNumber, int newLines, string leadingWhitespace, CodeUnit codeUnit)
#else
    public Token(string text, TokenType tokenType, bool wasEscaped, bool inDocComment, int lineNumber, int columnNumber, int newLines, string leadingWhitespace)
#endif
    {
        Text = text;
        TokenType = tokenType;
        WasEscaped = wasEscaped;
        _inDocComment = inDocComment;
        LineNumber = lineNumber;
        ColumnNumber = (ushort)columnNumber;
        NewLines = (ushort)newLines;
        LeadingWhitespace = leadingWhitespace;
#if DEBUG
        CodeUnit = codeUnit;
#endif
    }

#if DEBUG
    /// <summary>
    /// Enable this finalizer to trace lost comments
    /// </summary>
    ~Token()
    {
        if (TrailingComments != null && TrailingComments.Count > 0)
        {
            string error;
            CommentBase comment = TrailingComments[0];
            if (Text != null)
                error = "Line# " + LineNumber + ": LOST COMMENTS on token '" + Text + "'";
            else
                error = "Line# " + comment.LineNumber + ": LOST COMMENTS";
            error += ": \"" + comment.AsString().Split('\n')[0] + "\"";
            CodeUnit.LogAndAttachMessage(error, MessageSeverity.Error, MessageSource.Parse);
        }
    }
#endif

    #endregion

    #region /* PROPERTIES */

    /// <summary>
    /// Get the non-verbatim version of the text (without the '@' prefix, if any).
    /// </summary>
    public string NonVerbatimText
    {
        get { return (Text[0] == '@' ? Text.Substring(1) : Text); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is the first one on the current line.
    /// </summary>
    public bool IsFirstOnLine
    {
        get { return (NewLines > 0); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is an identifier.
    /// </summary>
    public bool IsIdentifier
    {
        get { return (TokenType == TokenType.Identifier); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is a symbol.
    /// </summary>
    public bool IsSymbol
    {
        get { return (TokenType == TokenType.Symbol); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is numeric.
    /// </summary>
    public bool IsNumeric
    {
        get { return (TokenType == TokenType.Numeric); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is a comment.
    /// </summary>
    public bool IsComment
    {
        get { return (TokenType == TokenType.Comment); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is the start of a documentation comment.
    /// </summary>
    public bool IsDocCommentStart
    {
        get { return (TokenType == TokenType.DocCommentStart); }
    }

    /// <summary>
    /// True if the <see cref="Token"/> is a documentation comment XML tag name.
    /// </summary>
    public bool IsDocCommentTag
    {
        get { return (TokenType == TokenType.DocCommentTag); }
    }

    /// <summary>
    /// True if inside a documentation comment.
    /// </summary>
    public override bool InDocComment
    {
        get { return _inDocComment; }
    }

    /// <summary>
    /// True if there are any trailing comments.
    /// </summary>
    public override bool HasTrailingComments
    {
        get { return (TrailingComments != null && TrailingComments.Count > 0); }
    }

    #endregion

    #region /* METHODS */

    /// <summary>
    /// Return this <see cref="Token"/>.
    /// </summary>
    public override Token AsToken()
    {
        return this;
    }

    /// <summary>
    /// Get this token as a string.
    /// </summary>
    public override string ToString()
    {
        return Text;
    }

    #endregion
}
