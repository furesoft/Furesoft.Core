// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Attribute = Furesoft.Core.CodeDom.CodeDOM.Annotations.Attribute;
using Furesoft.Core.CodeDom.Utilities;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using static Furesoft.Core.CodeDom.CodeDOM.Base.CodeObject;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;

namespace Furesoft.Core.CodeDom.Rendering;

/// <summary>
/// Used to format code objects as text and write them into a string or a file.
/// </summary>
/// <remarks>
/// The main purpose of this class is to keep track of the indentation level, and to emit the spaces
/// or tabs to create the indentation as the first text is written to each line.  It also supports
/// "pending comments" that are written in block style if not the last thing on the line.  And, it
/// keeps track of the current line number, and fires events when newlines are created.
/// </remarks>
public class CodeWriter : IDisposable
{
    /// <summary>
    /// C# keywords.
    /// </summary>
    public static HashSet<string> Keywords = new()
    {
            "abstract", "as",       "base",       "bool",      "break",
            "byte",     "case",     "catch",      "char",      "checked",
            "class",    "const",    "continue",   "decimal",   "default",
            "delegate", "do",       "double",     "else",      "enum",
            "event",    "explicit", "extern",     "false",     "finally",
            "fixed",    "float",    "for",        "foreach",   "goto",
            "if",       "implicit", "in",         "int",       "interface",
            "internal", "is",       "lock",       "long",      "namespace",
            "new",      "null",     "object",     "operator",  "out",
            "override", "params",   "private",    "protected", "public",
            "readonly", "ref",      "return",     "sbyte",     "sealed",
            "short",    "sizeof",   "stackalloc", "static",    "string",
            "struct",   "switch",   "this",       "throw",     "true",
            "try",      "typeof",   "uint",       "ulong",     "unchecked",
            "unsafe",   "ushort",   "using",      "virtual",   "void",
            "volatile", "while"
            // Many of these keywords could theoretically be contextual, but they're not historically, so we
            // have to treat them as keywords at least in text sources to be compatible with the C# compiler.
        };

    /// <summary>
    /// True if unicode characters should be escaped.
    /// </summary>
    public bool EscapeUnicode = true;

    /// <summary>
    /// True if rendering documentation comment content.
    /// </summary>
    public bool InDocCommentContent;

    /// <summary>
    /// True if tabs should be used for indentation instead of spaces.
    /// </summary>
    public bool UseTabs;

    protected Stack<AlignmentState> _alignmentStateStack = new(16);
    protected int _columnNumber = 1;
    protected bool _flushingEOLComments;
    protected Stack<IndentState> _indentStateStack = new(32);
    protected bool _isEmptyLine = true;
    protected bool _isGenerated;
    protected IndentState _lastPoppedIndentState;
    protected int _lineNumber = 1;
    protected List<EOLComment> _pendingEOLComments = new();
    protected TextWriter _textWriter;

    /// <summary>
    /// Create a code writer that writes to a string.
    /// </summary>
    public CodeWriter(bool calculateOnly, bool isGenerated)
    {
        if (!calculateOnly)
            _textWriter = new StringWriter();
        _indentStateStack.Push(new IndentState(null, 0, 0, 0));
        _isGenerated = isGenerated;
    }

    /// <summary>
    /// Create a code writer that writes to a string.
    /// </summary>
    public CodeWriter(bool calculateOnly)
        : this(calculateOnly, false)
    { }

    /// <summary>
    /// Create a code writer that writes to a string.
    /// </summary>
    public CodeWriter()
        : this(false, false)
    { }

    /// <summary>
    /// Create a code writer that writes to a text file.
    /// </summary>
    public CodeWriter(string fileName, Encoding encoding, bool hasUTF8BOM, bool useTabs, bool isGenerated)
    {
        // Create the specified output directory if it doesn't exist
        string directory = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Specify the encoding for the output, with special logic to omit the UTF8 BOM if it wasn't there originally
        FileStream fileStream = new(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
        if (encoding == Encoding.UTF8 && !hasUTF8BOM)
            encoding = new UTF8Encoding(false);
        _textWriter = new StreamWriter(fileStream, encoding);
        _indentStateStack.Push(new IndentState(null, 0, 0, 0));
        UseTabs = useTabs;
        _isGenerated = isGenerated;
    }

    /// <summary>
    /// Create a code writer that writes to a text file.
    /// </summary>
    public CodeWriter(string fileName, Encoding encoding, bool hasUTF8BOM, bool useTabs)
        : this(fileName, encoding, hasUTF8BOM, useTabs, false)
    { }

    /// <summary>
    /// This event is fired after a new line is created.
    /// </summary>
    public event Action<CodeWriter> AfterNewLine;

    /// <summary>
    /// A stack of <see cref="AlignmentState"/>s.
    /// </summary>
    public Stack<AlignmentState> AlignmentStateStack
    {
        get { return _alignmentStateStack; }
        set { _alignmentStateStack = value; }
    }

    /// <summary>
    /// The current column number (1 to N).
    /// </summary>
    public int ColumnNumber
    {
        get { return _columnNumber; }
    }

    /// <summary>
    /// Get or set the current indent offset (0 to N).
    /// </summary>
    public int IndentOffset
    {
        get { return _indentStateStack.Peek().IndentOffset; }
        set
        {
            _indentStateStack.Peek().IndentOffset = value;
            if (_isEmptyLine)
                _columnNumber = value + 1;
        }
    }

    /// <summary>
    /// True if the code being rendered is generated (such as a generated '.g.cs' file).  Code cleanup settings will be ignored.
    /// </summary>
    public bool IsGenerated
    {
        get { return _isGenerated; }
    }

    /// <summary>
    /// The current line number (1 to N).
    /// </summary>
    public int LineNumber
    {
        get { return _lineNumber; }
    }

    /// <summary>
    /// True if a newline is required before any other text, such as if a compiler directive was just emitted
    /// (used to force a newline before a terminating ';' on an expression with a compiler directive at the end).
    /// </summary>
    public bool NeedsNewLine { get; set; }

    /// <summary>
    /// Get or set the string used to create new lines (LF or CR/LF).
    /// </summary>
    public string NewLine
    {
        get { return (_textWriter != null ? _textWriter.NewLine : null); }
        set
        {
            if (_textWriter != null)
                _textWriter.NewLine = value;
        }
    }

    /// <summary>
    /// Begin the association of alignment information with a code object.
    /// </summary>
    public void BeginAlignment(CodeObject codeObject, int[] alignmentOffsets)
    {
        _alignmentStateStack.Push(new AlignmentState(codeObject, alignmentOffsets));
    }

    /// <summary>
    /// Begin a section during which any newline should be indented an extra level.
    /// </summary>
    public void BeginIndentOnNewLine(CodeObject codeObject)
    {
        IndentState indentState = _indentStateStack.Peek();
        _indentStateStack.Push(new IndentState(codeObject, indentState.IndentOffset,
            indentState.IndentOffset + CodeObject.TabSize, indentState.ParentOffset));
    }

    /// <summary>
    /// Begin a section during which any newline should be indented relative to the current offset.
    /// </summary>
    public void BeginIndentOnNewLineRelativeToCurrentOffset(CodeObject codeObject)
    {
        IndentState indentState = _indentStateStack.Peek();
        int newOffset = _columnNumber - 1;
        _indentStateStack.Push(new IndentState(codeObject, newOffset, newOffset, indentState.ParentOffset));
        IndentOffset = newOffset;  // Change current column now if line is empty
    }

    /// <summary>
    /// Begin a section during which any newline should be indented relative to the last indented offset.
    /// </summary>
    public void BeginIndentOnNewLineRelativeToLastIndent(CodeObject codeObject, CodeObject lastCodeObject)
    {
        // Use the last indented offset if the code object matches, otherwise just do a normal indent
        IndentState indentState = _lastPoppedIndentState;
        if (indentState == null || indentState.IndentObject != lastCodeObject)
            BeginIndentOnNewLine(codeObject);
        else
        {
            indentState.IndentObject = codeObject;
            _indentStateStack.Push(indentState);
        }
        IndentOffset = IndentOffset;  // Change current column now if line is empty
    }

    /// <summary>
    /// Begin a section during which any newline should be indented relative to the parent object offset.
    /// </summary>
    public void BeginIndentOnNewLineRelativeToParentOffset(CodeObject codeObject, bool additionalIndent)
    {
        IndentState indentState = _indentStateStack.Peek();
        _indentStateStack.Push(new IndentState(codeObject, indentState.IndentOffset,
            indentState.ParentOffset + (additionalIndent ? CodeObject.TabSize : 0), indentState.ParentOffset));
    }

    /// <summary>
    /// Begin a section during which any newline should be outdented by a certain amount, or to a certain offset.
    /// </summary>
    /// <param name="codeObject">The related code object.</param>
    /// <param name="offset">The indentation offset (0 to N), or amount to outdent if negative.</param>
    public void BeginOutdentOnNewLine(CodeObject codeObject, int offset)
    {
        IndentState indentState = _indentStateStack.Peek();
        int newOffset = (offset < 0 ? indentState.IndentOffsetOnNewLine + offset : offset);
        if (newOffset < 0) newOffset = 0;
        _indentStateStack.Push(new IndentState(codeObject, newOffset, newOffset, indentState.ParentOffset));
        IndentOffset = newOffset;  // Change current column now if line is empty
    }

    /// <summary>
    /// Dispose the object.
    /// </summary>
    public void Dispose()
    {
        if (_textWriter != null)
            _textWriter.Dispose();
    }

    /// <summary>
    /// End the association of alignment information with a code object.
    /// </summary>
    public void EndAlignment(CodeObject codeObject)
    {
        AlignmentState alignmentState = _alignmentStateStack.Pop();
        if (alignmentState.Object != codeObject)
            Log.WriteLine("ERROR popping alignment state stack - objects don't match!");
    }

    /// <summary>
    /// End a section during which any newline should be indented an extra level.
    /// </summary>
    public void EndIndentation(CodeObject codeObject)
    {
        _lastPoppedIndentState = _indentStateStack.Pop();
        if (_lastPoppedIndentState.IndentObject != codeObject)
            Log.WriteLine("ERROR popping indent state stack - objects don't match!");
        IndentOffset = IndentOffset;  // Change current column now if line is empty
    }

    /// <summary>
    /// Flush any pending data.
    /// </summary>
    public void Flush()
    {
        FlushPendingEOLComments(true);
    }

    /// <summary>
    /// Get the column width associated with the specified CodeObject.
    /// </summary>
    public int GetColumnWidth(CodeObject codeObject, int column)
    {
        foreach (AlignmentState alignmentState in _alignmentStateStack)
        {
            if (alignmentState.Object == codeObject && column < alignmentState.Offsets.Length)
                return alignmentState.Offsets[column];
        }
        return 0;
    }

    /// <summary>
    /// Get any column widths associated with the specified CodeObject.
    /// </summary>
    public int[] GetColumnWidths(CodeObject codeObject)
    {
        foreach (AlignmentState alignmentState in _alignmentStateStack)
        {
            if (alignmentState.Object == codeObject)
                return alignmentState.Offsets;
        }
        return null;
    }

    /// <summary>
    /// Get the indentation offset of the specified code object.
    /// </summary>
    public int GetIndentOffset(CodeObject codeObject)
    {
        foreach (IndentState indentState in _indentStateStack)
        {
            if (indentState.IndentObject == codeObject)
                return indentState.IndentOffset;
        }
        return 0;
    }

    /// <summary>
    /// Set the indent offset of the parent object.
    /// </summary>
    public void SetParentOffset()
    {
        _indentStateStack.Peek().ParentOffset = _columnNumber - 1;
    }

    /// <summary>
    /// Convert all written data to a string.
    /// </summary>
    public override string ToString()
    {
        return (_textWriter != null ? _textWriter.ToString() : "");
    }

    /// <summary>
    /// Write the specified text.
    /// </summary>
    public void Write(string text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        if (_pendingEOLComments.Count > 0)
            FlushPendingEOLComments(false);

        if (NeedsNewLine)
            WriteLine("");

        if (_isEmptyLine)
        {
            _isEmptyLine = false;

            // If we're writing the first text on the line, indent first
            int indentOffset = _indentStateStack.Peek().IndentOffset;
            if (_textWriter != null)
            {
                string indentString = (UseTabs ? new string('\t', indentOffset / CodeObject.TabSize) + new string(' ', indentOffset % CodeObject.TabSize)
                    : new string(' ', indentOffset));
                _textWriter.Write(indentString);
            }
            _columnNumber = indentOffset + 1;

            // If using tabs, also use tabs for any leading spaces in the text (this can occur in block comments
            // or skipped sections of conditional directives).
            if (_textWriter != null && UseTabs && text[0] == ' ')
            {
                int leadingSpaces = StringUtil.CharCount(text, ' ', 0);
                int tabs = leadingSpaces / CodeObject.TabSize;
                _textWriter.Write(new string('\t', tabs));
                int offset = tabs * CodeObject.TabSize;
                _columnNumber += offset;
                text = text.Substring(offset);
            }
        }

        // Scan the output text for any required encoding
        StringBuilder result = null;
        int i, start = 0;
        for (i = 0; i < text.Length; ++i)
        {
            string encoded = null;
            if (InDocCommentContent)
            {
                // Encode any '&', '<', '>' chars if we're in doc comment content
                switch (text[i])
                {
                    case '&': encoded = "&amp;"; break;
                    case '<': encoded = "&lt;"; break;
                    case '>': encoded = "&gt;"; break;
                }
            }

            // Encode any unicode chars if so requested
            if (text[i] > 0xff && EscapeUnicode)
            {
                if (char.IsSurrogatePair(text, i))
                {
                    int u32 = char.ConvertToUtf32(text, i);
                    encoded = string.Format(@"\U{0:x8}", u32);
                }
                else
                    encoded = string.Format(@"\u{0:x4}", (int)text[i]);
            }

            if (encoded != null)
            {
                if (result == null)
                    result = new StringBuilder();
                result.Append(text, start, i - start);
                result.Append(encoded);
                start = i + 1;
            }
        }
        if (result != null)
            text = result.Append(text, start, i - start).ToString();

        // Write the text
        if (_textWriter != null)
            _textWriter.Write(text);
        _columnNumber += text.Length;
    }

    /// <summary>
    /// Write an identifier, prefixing with '@' if it happens to be a keyword.
    /// </summary>
    public void WriteIdentifier(string text, RenderFlags flags)
    {
        // Delimit the identifier if we're not in a doc comment and it's the same as a C# keyword
        if (!flags.HasFlag(RenderFlags.InDocComment) && Keywords.Contains(text))
            Write("@");
        Write(text);
    }

    /// <summary>
    /// Write optional text followed by a newline.
    /// </summary>
    public void WriteLine(string text)
    {
        if (text != null)
            Write(text);

        if (_pendingEOLComments.Count > 0)
            FlushPendingEOLComments(true);

        if (_textWriter != null)
            _textWriter.WriteLine();
        ++_lineNumber;
        _columnNumber = 1;
        _isEmptyLine = true;
        NeedsNewLine = false;

        // Set any new indentation for the new line
        IndentOffset = _indentStateStack.Peek().IndentOffsetOnNewLine;

        AfterNewLine?.Invoke(this);
    }

    /// <summary>
    /// Write optional text followed by a newline.
    /// </summary>
    public void WriteLine()
    {
        WriteLine(null);
    }

    /// <summary>
    /// Write the specified number of newlines.
    /// </summary>
    public void WriteLines(int count)
    {
        for (int i = 0; i < count; ++i)
            WriteLine();
    }

    /// <summary>
    /// Write a list of CodeObjects.
    /// </summary>
    public void WriteList<T>(IEnumerable<T> enumerable, RenderFlags flags, CodeObject parent, int[] columnWidths) where T : CodeObject
    {
        if (enumerable == null)
            return;

        // Increase the indent level for any newlines that occur within the child list unless specifically told not to
        bool increaseIndent = !flags.HasFlag(RenderFlags.NoIncreaseIndent);
        if (increaseIndent)
            BeginIndentOnNewLine(parent);

        // Render the items in the list
        bool isSingleLine = true;
        bool isFirst = true;
        int column = 0;
        IEnumerator<T> enumerator = enumerable.GetEnumerator();
        bool hasMore = enumerator.MoveNext();
        while (hasMore)
        {
            CodeObject codeObject = enumerator.Current;
            hasMore = enumerator.MoveNext();
            if (codeObject != null)
            {
                if (codeObject.IsFirstOnLine)
                {
                    isSingleLine = false;
                    column = 0;
                }

                // Render any newlines here, so that the indentation will be correct if the object has any post annotations
                // (which are rendered separately below, so that any commas can be rendered properly).
                if (!flags.HasFlag(RenderFlags.IsPrefix) && !flags.HasFlag(RenderFlags.SuppressNewLine) && codeObject.NewLines > 0)
                {
                    WriteLines(codeObject.NewLines);
                    // Set the parent offset for any post annotations
                    SetParentOffset();
                }

                // Render the code object, omitting any EOL comments and post annotations (so they can be rendered later after
                // any comma), and prefixing a space if it's not the first item.
                RenderFlags passFlags = flags | RenderFlags.NoEOLComments | RenderFlags.NoPostAnnotations | RenderFlags.SuppressNewLine | (isSingleLine ? (isFirst ? 0 : RenderFlags.PrefixSpace) : (codeObject.IsFirstOnLine ? 0 : RenderFlags.PrefixSpace));
                codeObject.AsText(this, passFlags);
                flags &= ~(RenderFlags.SuppressNewLine | RenderFlags.NoPreAnnotations);

                if (hasMore)
                {
                    // Render the trailing comma, with any EOL comments before or after it, depending on whether or not it's the last thing on the line
                    CodeObject nextObject = enumerator.Current;
                    bool isLastOnLine = (nextObject != null && nextObject.IsFirstOnLine);
                    if (!isLastOnLine)
                        codeObject.AsTextEOLComments(this, flags);
                    if (!flags.HasFlag(RenderFlags.NoItemSeparators))
                        Write(Expression.ParseTokenSeparator);
                    if (!isLastOnLine || codeObject.HasEOLComments)
                        WritePadding(codeObject, columnWidths, column);
                    if (isLastOnLine)
                        codeObject.AsTextEOLComments(this, flags);
                }
                else
                {
                    if (flags.HasFlag(RenderFlags.HasTerminator) && !flags.HasFlag(RenderFlags.Description) && parent != null)
                        ((Statement)parent).AsTextTerminator(this, flags);
                    bool hasCloseBraceOnSameLine = (parent is Initializer && !((Initializer)parent).IsEndFirstOnLine);
                    if (codeObject.HasEOLComments || hasCloseBraceOnSameLine)
                    {
                        WritePadding(codeObject, columnWidths, column);

                        // If we're aligning columns and it's a multi-line list, add an extra space to line up the EOL comment
                        // or close brace since there's no trailing comma.
                        if (columnWidths != null && !isSingleLine)
                        {
                            if (_textWriter != null)
                                _textWriter.Write(' ');
                            ++_columnNumber;
                        }
                    }
                    codeObject.AsTextEOLComments(this, flags);
                }
                codeObject.AsTextAnnotations(this, AnnotationFlags.IsPostfix, flags);
            }
            else if (hasMore)
                Write(Expression.ParseTokenSeparator);

            isFirst = false;
            ++column;
        }

        // Reset the indent level
        if (increaseIndent)
            EndIndentation(parent);
    }

    /// <summary>
    /// Write a list of CodeObjects.
    /// </summary>
    public void WriteList<T>(IEnumerable<T> enumerable, RenderFlags flags, CodeObject parent) where T : CodeObject
    {
        WriteList(enumerable, flags, parent, null);
    }

    /// <summary>
    /// Render a name, hiding any 'Attribute' suffix if it's an attribute name.
    /// </summary>
    public void WriteName(string name, RenderFlags flags, bool possibleKeyword)
    {
        // Hide any "Attribute" suffix for attribute constructor names
        if (flags.HasFlag(RenderFlags.Attribute) && name.EndsWith(Attribute.NameSuffix))
        {
            name = name.Substring(0, name.Length - Attribute.NameSuffix.Length);
            possibleKeyword = false;
        }
        if (possibleKeyword)
            WriteIdentifier(name, flags);
        else
            Write(name);
    }

    /// <summary>
    /// Render a name, hiding any 'Attribute' suffix if it's an attribute name.
    /// </summary>
    public void WriteName(string name, RenderFlags flags)
    {
        WriteName(name, flags, false);
    }

    /// <summary>
    /// Write a pending EOL comment, to be flushed later once it's known if anything follows it on the same line.
    /// </summary>
    public void WritePendingEOLComment(Comment comment, RenderFlags flags)
    {
        // If the EOL comment is on a new line (postfix comment), flush any existing pending EOL comments first
        if (comment.IsFirstOnLine && _pendingEOLComments.Count > 0)
            FlushPendingEOLComments(true);

        // Save the comment until the next write, so we can determine if it's the last thing on the line.
        // Also save the indentation of the parent, in case it's actually a postfix comment on a new line (for
        // an Expression).  Use the max of any ParentOffset or the current IndentOffset.
        // Also save the rendering flags, to preserve flags such as UpdateLineCol for later rendering.
        int parentOffset = _indentStateStack.Peek().ParentOffset;
        _pendingEOLComments.Add(new EOLComment(comment, Math.Max(parentOffset, IndentOffset), flags & RenderFlags.PassMask));
    }

    protected void FlushPendingEOLComments(bool isEndOfLine)
    {
        if (!_flushingEOLComments)  // Prevent re-entry
        {
            // Preserve 'NeedsNewLine' state and clear it during this operation
            bool needsNewLine = NeedsNewLine;
            NeedsNewLine = false;
            _flushingEOLComments = true;
            foreach (EOLComment eolComment in _pendingEOLComments)
            {
                // If the comment is the first thing on the line, we have to restore the original indentation
                // offset temporarily before rendering it.
                Comment comment = eolComment.Comment;
                if (comment.IsFirstOnLine)
                    BeginOutdentOnNewLine(comment, eolComment.Indentation);
                comment.AsText(this, eolComment.RenderFlags | (isEndOfLine ? 0 : RenderFlags.CommentsInline));
                if (comment.IsFirstOnLine)
                {
                    EndIndentation(comment);
                    needsNewLine = false;  // Force off if we emitted a newline
                }
            }
            _pendingEOLComments.Clear();
            _flushingEOLComments = false;
            NeedsNewLine = needsNewLine;
        }
    }

    private void WritePadding(CodeObject codeObject, int[] columnWidths, int column)
    {
        if (columnWidths != null && column < columnWidths.Length)
        {
            int paddingLength = columnWidths[column] - codeObject.AsTextLength(RenderFlags.LengthFlags, AlignmentStateStack);
            if (paddingLength > 0)
            {
                if (_textWriter != null)
                    _textWriter.Write(new string(' ', paddingLength));
                _columnNumber += paddingLength;
            }
        }
    }

    /// <summary>
    /// Alignment state information related to a <see cref="CodeObject"/>.
    /// </summary>
    public class AlignmentState
    {
        /// <summary>
        /// The <see cref="CodeObject"/>.
        /// </summary>
        public CodeObject Object;

        /// <summary>
        /// Alignment offsets.
        /// </summary>
        public int[] Offsets;

        /// <summary>
        /// Create an <see cref="AlignmentState"/>.
        /// </summary>
        public AlignmentState(CodeObject obj, int[] offsets)
        {
            Object = obj;
            Offsets = offsets;
        }
    }

    /// <summary>
    /// EOL comment and flags for delayed rendering.
    /// </summary>
    public class EOLComment
    {
        /// <summary>
        /// The EOL <see cref="Comment"/>.
        /// </summary>
        public Comment Comment;

        /// <summary>
        /// The indentation of the comment.
        /// </summary>
        public int Indentation;

        /// <summary>
        /// The rendering flags.
        /// </summary>
        public RenderFlags RenderFlags;

        /// <summary>
        /// Create an <see cref="EOLComment"/>.
        /// </summary>
        public EOLComment(Comment comment, int indentation, RenderFlags flags)
        {
            Comment = comment;
            Indentation = indentation;
            RenderFlags = flags;
        }
    }

    /// <summary>
    /// State information for the current indentation offset (related to a CodeObject).
    /// </summary>
    protected class IndentState
    {
        public CodeObject IndentObject;
        public int IndentOffset;
        public int IndentOffsetOnNewLine;
        public int ParentOffset;

        public IndentState(CodeObject indentObject, int indentOffset, int indentOffsetOnNewLine, int parentOffset)
        {
            IndentObject = indentObject;
            IndentOffset = (indentOffset >= 0 ? indentOffset : 0);
            IndentOffsetOnNewLine = (indentOffsetOnNewLine >= 0 ? indentOffsetOnNewLine : 0);
            ParentOffset = parentOffset;
        }
    }
}