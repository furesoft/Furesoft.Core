// The Nova Project by Ken Beckett.
// Copyright (C) 2007-2012 Inevitable Software, all rights reserved.
// Released under the Common Development and Distribution License, CDDL-1.0: http://opensource.org/licenses/cddl1.php

using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments.Other;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;
using Furesoft.Core.CodeDom.Parsing.Base;
using Furesoft.Core.CodeDom.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Furesoft.Core.CodeDom.Parsing;

/// <summary>
/// Supports the parsing of text into code objects.
/// </summary>
/// <remarks>
/// Code objects have the ability to parse themselves from text, but they use this class to
/// help them do it.  An instance of this class is created for each file (or chunk of text)
/// being parsed, and it tokenizes the text and calls back to the logic in the code objects
/// via registered "parse-points".
/// </remarks>
public class Parser : IDisposable
{
    /// <summary>
    /// The <see cref="CodeUnit"/> being parsed.
    /// </summary>
    public CodeUnit CodeUnit;

    /// <summary>
    /// Global parsing flags.
    /// </summary>
    public ParseFlags ParseFlags;

    /// <summary>
    /// The current line number.
    /// </summary>
    public int LineNumber;

    /// <summary>
    /// The text of the current token (null if none).
    /// </summary>
    public string TokenText;

    /// <summary>
    /// The type of the current token (None if none).
    /// </summary>
    public TokenType TokenType;

    public static void AddMultipleParsePoints(string[] startTokens, ParseDelegate callback)
    {
        foreach (var item in startTokens)
        {
            Parser.AddParsePoint(item, callback);
        }
    }

    public static void Clear()
    {
        _operatorInfoMap.Clear();
        _tokenMap.Clear();
    }

    /// <summary>
    /// The last token.
    /// </summary>
    public Token LastToken;

    /// <summary>
    /// The starting token of the last parent code object.
    /// </summary>
    public Token ParentStartingToken;

    /// <summary>
    /// The current nesting level if parsing the '?' clause of a <see cref="Conditional"/> expression.
    /// </summary>
    public int ConditionalNestingLevel;

    /// <summary>
    /// SLOC count: Lines with other than blanks, comments, symbols (such as braces).
    /// </summary>
    public int SLOC;

    /// <summary>
    /// Delegate for parsing single unused identifiers in Blocks (used for EnumDecls).
    /// </summary>
    public Func<Parser, CodeObject, ParseFlags, CodeObject> SingleUnusedIdentifierParser;

    /// <summary>
    /// Unused tokens or code objects in the current active scope.
    /// </summary>
    public List<ParsedObject> Unused;

    /// <summary>
    /// Special-case Unused objects after the current code object instead of before it.
    /// </summary>
    public List<ParsedObject> PostUnused;

    // Stack of unused object lists (one for each active scope)
    protected readonly Stack<List<ParsedObject>> _unusedListStack = new();

    // List of tokens pre-fetched by the peek-ahead logic
    protected readonly List<Token> _peekAheadTokens = new();

    protected int _peekAheadIndex;

    // Stack of objects at which bubble-up normalization of EOL comments should stop
    protected readonly Stack<CodeObject> NormalizationBlockerStack = new();

    // Stack of Conditional expression nesting level counts
    protected readonly Stack<int> ConditionalNestingLevelStack = new();

    protected TextReader _textReader;  // The text stream being parsed
    protected string _line;            // The current line of text
    protected int _startLine;          // The starting line number
    protected int _length;             // The length of the current line
    protected int _start, _pos;        // The start and current position of the text being tokenized
    protected char _ch;                // The current character being examined
    protected char _pk;                // The next character in the stream
    protected bool _wasEscaped;        // True if the character was escaped, and translated
    protected string _previous;        // Data from previous lines for current token
    protected Token _token;            // The current token
    protected int _linesWithSpaces;    // Number of lines starting with spaces
    protected int _linesWithTabs;      // Number of lines starting with tabs
    protected bool _isSLOC;            // True if the current line should be counted towards the SLOC total

    /// <summary>
    /// Parse source code from the specified CodeUnit.
    /// </summary>
    public Parser(CodeUnit codeUnit, ParseFlags flags, bool isGenerated)
    {
        CodeUnit = codeUnit;
        ParseFlags = flags;
        IsGenerated = isGenerated;
        if (codeUnit.IsFile)
        {
            FileStream fileStream = new(codeUnit.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bom = new byte[3];
            fileStream.Read(bom, 0, 3);
            if (bom[0] == 0xEF && bom[1] == 0xBB && bom[2] == 0xBF)
                codeUnit.FileHasUTF8BOM = true;
            fileStream.Position = 0;
            StreamReader streamReader = new(fileStream, true);
            streamReader.Peek();  // Peek at the first char so that the encoding is determined
            codeUnit.FileEncoding = streamReader.CurrentEncoding;
            _textReader = streamReader;
        }
        else
            _textReader = new StringReader(codeUnit.Code);

        // Prime the parser by reading the first token
        NextToken();
    }

    /// <summary>
    /// Parse source code from the specified CodeUnit.
    /// </summary>
    public Parser(CodeUnit codeUnit, ParseFlags flags)
        : this(codeUnit, flags, false)
    { }

    /// <summary>
    /// Parse source code from the specified CodeUnit.
    /// </summary>
    public Parser(CodeUnit codeUnit)
        : this(codeUnit, ParseFlags.None, false)
    { }

    /// <summary>
    /// The current token.
    /// </summary>
    public Token Token
    {
        get { return _token; }
        protected set
        {
            LastToken = _token;
            _token = value;
            if (_token == null)
            {
                // Setup shortcut fields so client code doesn't have to check for null
                TokenText = null;
                TokenType = TokenType.None;
            }
            else
            {
                // Setup shortcut fields so client code doesn't have to check for null
                TokenText = _token.Text;
                TokenType = _token.TokenType;
            }
        }
    }

    /// <summary>
    /// The next character to be processed.
    /// </summary>
    public char Char
    {
        get { return _ch; }
    }

    /// <summary>
    /// The character after the next character to be processed.
    /// </summary>
    public char PeekChar
    {
        get { return _pk; }
    }

    /// <summary>
    /// Get the last parsed object in the Unused list (Token or UnusedCodeObject).
    /// </summary>
    public ParsedObject LastUnused
    {
        get { return ((Unused != null && Unused.Count > 0) ? Unused[Unused.Count - 1] : null); }
    }

    /// <summary>
    /// Get the last parsed object in the Unused list as a Token (null if none, or not a Token).
    /// </summary>
    public Token LastUnusedToken
    {
        get
        {
            ParsedObject lastUnused = LastUnused;
            return (lastUnused is Token ? (Token)lastUnused : null);
        }
    }

    /// <summary>
    /// Get the text of the last parsed object in the Unused list as a Token (null if none).
    /// </summary>
    public string LastUnusedTokenText
    {
        get
        {
            ParsedObject lastUnused = LastUnused;
            return (lastUnused is Token ? ((Token)lastUnused).Text : null);
        }
    }

    /// <summary>
    /// Get the last parsed object in the Unused list as a CodeObject (null if none, or not an UnusedCodeObject).
    /// </summary>
    public CodeObject LastUnusedCodeObject
    {
        get
        {
            ParsedObject lastUnused = LastUnused;
            return (lastUnused is UnusedCodeObject ? ((UnusedCodeObject)lastUnused).CodeObject : null);
        }
    }

    /// <summary>
    /// Returns true if there are any Unused items.
    /// </summary>
    public bool HasUnused
    {
        get { return (Unused != null && Unused.Count > 0); }
    }

    /// <summary>
    /// Returns true if the last Unused item is a Token.
    /// </summary>
    public bool HasUnusedToken
    {
        get { return (LastUnused is Token); }
    }

    /// <summary>
    /// Returns true if the last Unused item is a Token which is an Identifier.
    /// </summary>
    public bool HasUnusedIdentifier
    {
        get
        {
            ParsedObject lastUnused = LastUnused;
            return ((lastUnused is Token) && ((Token)lastUnused).IsIdentifier);
        }
    }

    /// <summary>
    /// Returns true if the last Unused item is a valid Expression or a token that is an identifier.
    /// </summary>
    public bool HasUnusedExpression
    {
        get
        {
            ParsedObject lastUnused = LastUnused;
            return ((lastUnused is UnusedCodeObject && ((UnusedCodeObject)lastUnused).CodeObject is Expression)
                || (lastUnused is Token && ((Token)lastUnused).IsIdentifier));
        }
    }

    /// <summary>
    /// Return true if the last unused item is a TypeRef or possible TypeRef.
    /// </summary>
    public bool HasUnusedTypeRef
    {
        get { return (HasUnused && CheckForUnusedTypeRef(0)); }
    }

    /// <summary>
    /// Return true if the last 2 unused items are a TypeRef or possible TypeRef followed by an identifier.
    /// </summary>
    public bool HasUnusedTypeRefAndIdentifier
    {
        get { return (Unused.Count >= 2 && HasUnusedIdentifier && CheckForUnusedTypeRef(-1)); }
    }

    /// <summary>
    /// Return true if the last 2 unused items are a TypeRef or possible TypeRef followed by an Expression.
    /// </summary>
    public bool HasUnusedTypeRefAndExpression
    {
        get { return (Unused.Count >= 2 && HasUnusedExpression && CheckForUnusedTypeRef(-1)); }
    }

    /// <summary>
    /// Get the line number of the current token.
    /// </summary>
    public int TokenLineNumber
    {
        get { return (_token == null ? LineNumber : _token.LineNumber); }
    }

    /// <summary>
    /// Get the last token that was returned by PeekNextToken().
    /// </summary>
    public Token LastPeekedToken
    {
        get
        {
            int index = _peekAheadIndex - 1;
            return (index >= 0 && index < _peekAheadTokens.Count ? _peekAheadTokens[index] : null);
        }
    }

    /// <summary>
    /// Get the text of the last token that was returned by PeekNextToken().
    /// </summary>
    public string LastPeekedTokenText
    {
        get
        {
            Token token = LastPeekedToken;
            return (token != null ? token.Text : null);
        }
    }

    /// <summary>
    /// True if parsing a generated file (such as '.g.cs' or '.Designer.cs').  Formatting and code cleanup settings will be ignored.
    /// </summary>
    public bool IsGenerated { get; set; }

    /// <summary>
    /// True if parsing a region of generated code (such as code from the MS Component Designer).  Formatting and code cleanup settings will be ignored.
    /// </summary>
    public bool IsGeneratedRegion { get; set; }

    /// <summary>
    /// Add a code object that was just parsed but not yet needed to the unused list.
    /// </summary>
    public void AddUnused(CodeObject codeObject)
    {
        Unused.Add(new UnusedCodeObject(codeObject, LastToken));
    }

    /// <summary>
    /// Add a token that was just parsed but not yet needed to the unused list.
    /// </summary>
    public void AddUnused(Token token)
    {
        // Update the parent starting token (used by indentation logic)
        ParentStartingToken = token;
        Unused.Add(token);
    }

    /// <summary>
    /// Get the object in the Unused list at the specified index.
    /// </summary>
    public ParsedObject GetUnused(int index)
    {
        return (index >= 0 && index < Unused.Count ? Unused[index] : null);
    }

    /// <summary>
    /// Get the object in the Unused list at the specified index as a CodeObject.
    /// </summary>
    public CodeObject GetUnusedCodeObject(int index)
    {
        if (index >= 0 && index < Unused.Count)
        {
            ParsedObject unused = Unused[index];
            if (unused is UnusedCodeObject)
                return ((UnusedCodeObject)unused).CodeObject;
        }
        return null;
    }

    /// <summary>
    /// Move the current unused list to the post-unused list.
    /// </summary>
    public void MoveUnusedToPostUnused()
    {
        PostUnused = Unused;
        Unused = new List<ParsedObject>();
    }

    /// <summary>
    /// Move the current post-unused list to the unused list.
    /// </summary>
    public void MovePostUnusedToUnused()
    {
        Unused = PostUnused;
        PostUnused = null;
    }

    /// <summary>
    /// Push an object that will block EOL comment bubble-up normalization.
    /// </summary>
    public void PushNormalizationBlocker(CodeObject codeObject)
    {
        NormalizationBlockerStack.Push(codeObject);
        ConditionalNestingLevelStack.Push(ConditionalNestingLevel);
        ConditionalNestingLevel = 0;
    }

    /// <summary>
    /// Pop the last normalization blocker object.
    /// </summary>
    public void PopNormalizationBlocker()
    {
        NormalizationBlockerStack.Pop();
        ConditionalNestingLevel = ConditionalNestingLevelStack.Pop();
    }

    /// <summary>
    /// Get the current normalization blocker object.
    /// </summary>
    public CodeObject GetNormalizationBlocker()
    {
        return NormalizationBlockerStack.Peek();
    }

    /// <summary>
    /// Check that there appears to be an unused identifier, TypeRef, or UnresolvedRef at the specified offset.
    /// </summary>
    protected bool CheckForUnusedTypeRef(int offset)
    {
        ParsedObject nextToLast = GetUnused(Unused.Count - 1 + offset);
        if (nextToLast is Token && ((Token)nextToLast).IsIdentifier)
            return true;
        if (nextToLast is UnusedCodeObject)
        {
            CodeObject codeObject = ((UnusedCodeObject)nextToLast).CodeObject;
            if (codeObject is TypeRef || codeObject is UnresolvedRef)
                return true;

            // Also allow a Dot operator whose right-most operand is a TypeRef or UnresolvedRef
            if (codeObject is Dot)
            {
                Expression right = ((Dot)codeObject).SkipPrefixes();
                if (right is TypeRef || right is UnresolvedRef)
                    return true;
            }

            // Also allow a Lookup operator
            if (codeObject is Lookup)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Process the current token using the specified flags, returning a CodeObject if successful,
    /// otherwise returns null and the unrecognized token is saved in the Unused list for later.
    /// </summary>
    public CodeObject ProcessToken(CodeObject parent, ParseFlags flags)
    {
        CodeObject obj = null;
        flags |= ParseFlags;  // Add in any global flags

        // Reset the peek-ahead queue index any time we process a token, so that we can peek-ahead
        // while parsing even if we've already done it for the same tokens.
        _peekAheadIndex = 0;

        try
        {
            // Handle direct mappings for some token types
            switch (_token.TokenType)
            {
                case TokenType.String:
                case TokenType.VerbatimString:
                case TokenType.Char:
                case TokenType.Numeric:
                    obj = new Literal(this, parent);
                    break;

                case TokenType.Comment:
                    obj = new Comment(this, parent);
                    break;

                case TokenType.DocCommentStart:
                    obj = DocComment.Parse(this, parent, flags);
                    break;

                case TokenType.DocCommentTag:
                    // If it's a DocComment open-tag, look for a parse-point
                    if (LastToken.Text == DocComment.ParseTokenTagOpen)
                    {
                        if (_docCommentTagMap.TryGetValue(TokenText, out ParseDelegate @delegate))
                            obj = @delegate(this, parent, flags);
                        else
                            obj = new DocTag(TokenText, this, parent);
                    }
                    // If we didn't recognize the token, save it in the Unused list and move ahead to the next one
                    if (obj == null)
                        SaveAndNextToken();
                    break;

                case TokenType.Identifier:
                case TokenType.Symbol:
                    {
                        // If it's an Identifier or Symbol, look for a parse-point
                        if (_tokenMap.TryGetValue(TokenText, out List<ParsePoint> list))
                        {
                            // Check each parse-point in priority order until one parses successfully
                            foreach (ParsePoint parsePoint in list)
                            {
                                // Only call the parse-point if we're in a valid context
                                if (parsePoint.IsContextValid(this, parent, flags))
                                {
                                    // Callback to the static code object method, so it can parse itself
                                    obj = parsePoint.Callback(this, parent, flags);

                                    // Reset the peek-ahead queue between attempts
                                    _peekAheadIndex = 0;

                                    if (obj != null)
                                        break;
                                }
                            }
                        }
                        // If we didn't recognize the token, save it in the Unused list and move ahead to the next one
                        if (obj == null)
                        {
                            // Special logic to ignore any extraneous empty statements (';')
                            if (TokenText == Statement.ParseTokenTerminator)
                            {
                                // Discard token and get any comment as a non-EOL comment
                                NextToken();
                                List<CommentBase> comments = LastToken.TrailingComments;
                                if (comments != null && comments.Count > 0)
                                {
                                    CommentBase comment = comments[0];
                                    comment.IsEOL = false;
                                    comment.NewLines += LastToken.NewLines;
                                    comments.RemoveAt(0);
                                    obj = comment;
                                }
                            }
                            else
                            {
                                SaveAndNextToken();

                                // Do a special check for a single unused identifier in a Block (used for EnumMemberDecls)
                                if (TokenText == Block.ParseTokenEnd && LastToken.IsIdentifier && SingleUnusedIdentifierParser != null)
                                {
                                    obj = SingleUnusedIdentifierParser(this, parent, ParseFlags.None);
                                }
                                // Do a special check for "Type`N" style names embedded in doc comments, and convert to "Type<>"
                                else if (TokenText == "`" && _inDocCommentCodeContent)
                                {
                                    int number = int.Parse(PeekNextTokenText());
                                    if (number > 0)
                                    {
                                        NextToken(); // Skip past '`'
                                        NextToken(); // Skip past the number token
                                        obj = new UnresolvedRef(RemoveLastUnusedToken()) { TypeArguments = ChildList<Expression>.CreateListOfNulls(number) };
                                    }
                                }
                            }
                        }
                        break;
                    }
                case TokenType.CompilerDirective:
                    {
                        // If it's a compiler directive start symbol, look for a parse-point
                        Token next = PeekNextToken();
                        if (next != null && !next.IsFirstOnLine && next.TokenType == TokenType.Identifier)
                        {
                            if (_compilerDirectiveMap.TryGetValue(next.Text, out ParseDelegate @delegate))
                                obj = @delegate(this, parent, flags);
                        }
                        // If we didn't recognize the compiler directive, save the '#' in the Unused list and move ahead to the next token
                        if (obj == null)
                            SaveAndNextToken();
                        break;
                    }
                default:
                    // For unrecognized token types, save them in the unused list
                    SaveAndNextToken();
                    break;
            }
        }
        catch (Exception ex)
        {
            // Move past the offending token, create a comment to mark the problem, and log the exception
            Token badToken = Token;
            string tokenText = TokenText;
            NextToken();
            const int MaxBadTokenDisplayLength = 35;
            string badTokenText = "'" + (tokenText != null && tokenText.Length > MaxBadTokenDisplayLength ? tokenText.Substring(0, MaxBadTokenDisplayLength) + "..." : tokenText) + "'";
            string message = CodeUnit.LogException(ex, "parsing token " + badTokenText);
            obj = new Comment(message) { NewLines = 2, Parent = parent };
            AttachMessage(obj, message, badToken);
        }

        return obj;
    }

    /// <summary>
    /// Process the current token using the specified flags, returning a CodeObject if successful,
    /// otherwise returns null and the unrecognized token is saved in the Unused list for later.
    /// </summary>
    public CodeObject ProcessToken(CodeObject parent)
    {
        return ProcessToken(parent, ParseFlags.None);
    }

    /// <summary>
    /// Reset the peek ahead queue index.
    /// </summary>
    public void ResetPeekAhead()
    {
        _peekAheadIndex = 0;
    }

    /// <summary>
    /// Save the current token in the Unused list, and get the next one.
    /// </summary>
    public Token SaveAndNextToken()
    {
        AddUnused(_token);
        return NextToken();
    }

    /// <summary>
    /// Remove the last object from the Unused list.
    /// </summary>
    public ParsedObject RemoveLastUnused()
    {
        ParsedObject unused = null;
        if (Unused != null && Unused.Count > 0)
        {
            int index = Unused.Count - 1;
            unused = Unused[index];
            Unused.RemoveAt(index);
        }
        return unused;
    }

    /// <summary>
    /// Remove the last object from the Unused list, IF it's a Token.
    /// </summary>
    public Token RemoveLastUnusedToken()
    {
        Token token = null;
        if (Unused != null && Unused.Count > 0)
        {
            int index = Unused.Count - 1;
            ParsedObject unused = Unused[index];
            if (unused is Token)
            {
                token = (Token)unused;
                Unused.RemoveAt(index);
            }
        }
        return token;
    }

    /// <summary>
    /// Remove the last object from the Unused list, converted to an Expression.
    /// </summary>
    /// <param name="force">True to force non-identifier tokens to be removed.</param>
    public Expression RemoveLastUnusedExpression(bool force)
    {
        Expression expression = null;

        if (Unused != null && Unused.Count > 0)
        {
            int index = Unused.Count - 1;
            ParsedObject lastUnused = Unused[index];
            if (lastUnused is Token && (((Token)lastUnused).IsIdentifier || force))
            {
                Token token = (Token)lastUnused;

                // If we're in a directive expression, create a DirectiveSymbolRef now, otherwise create an UnresolvedRef
                expression = (InDirectiveExpression ? (Expression)new DirectiveSymbolRef(token) : new UnresolvedRef(token));

                // Move any trailing EOL or inline comment to the expression as an EOL comment
                expression.MoveEOLComment(token);
            }
            else if (lastUnused is UnusedCodeObject && ((UnusedCodeObject)lastUnused).CodeObject is Expression)
                expression = (Expression)((UnusedCodeObject)lastUnused).CodeObject;
            else if (lastUnused is UnusedCodeObject && ((UnusedCodeObject)lastUnused).CodeObject is Comment && index > 0)
            {
                // If the last unused was a comment, and a previous unused expression exists, do special handling
                ParsedObject nextUnused = Unused[index - 1];
                if (nextUnused is UnusedCodeObject && ((UnusedCodeObject)nextUnused).CodeObject is Expression)
                {
                    expression = (Expression)((UnusedCodeObject)nextUnused).CodeObject;
                    expression.AttachAnnotation((Comment)((UnusedCodeObject)lastUnused).CodeObject, AnnotationFlags.IsPostfix);
                    Unused.RemoveAt(index);
                    --index;
                }
            }
            if (expression != null)
                Unused.RemoveAt(index);
        }

        return expression;
    }

    /// <summary>
    /// Remove the last object from the Unused list, converted to an Expression.
    /// Does NOT remove tokens that aren't valid identifiers.
    /// </summary>
    public Expression RemoveLastUnusedExpression()
    {
        return RemoveLastUnusedExpression(false);
    }

    /// <summary>
    /// Push the current Unused list.
    /// </summary>
    public void PushUnusedList()
    {
        _unusedListStack.Push(Unused);
        Unused = new List<ParsedObject>();
    }

    /// <summary>
    /// Restore the previous Unused list.
    /// </summary>
    public void PopUnusedList()
    {
        if (Unused != null && Unused.Count > 0)
            throw new Exception("Popping Unused list, but current list isn't empty!");
        if (_unusedListStack.Count == 0)
            throw new Exception("Popping Unused list, but UnusedListStack is empty!");
        Unused = _unusedListStack.Pop();
    }

    /// <summary>
    /// Peek ahead at the next unparsed token, skipping comments.
    /// Peek-ahead tokens are saved in a special queue so they are still processed by NextToken().
    /// Returns null on EOF or if a compiler directive ('#') is encountered.
    /// </summary>
    public Token PeekNextToken()
    {
        Token token;
        do
        {
            if (_peekAheadIndex < _peekAheadTokens.Count)
                token = _peekAheadTokens[_peekAheadIndex++];
            else
            {
                token = ParseNextToken();
                if (token == null) break;
                AddPeekAheadToken(token);

                // Abort if we peeked at a '#' - we can't peek-ahead through a compiler
                // directive because they aren't read as tokens (many of them read to EOF,
                // especially for text excluded by the conditionals).
                if (token.Text == CompilerDirective.ParseToken)
                    return null;
            }
        }
        while (token.IsComment);  // Skip past comments

        return token;
    }

    /// <summary>
    /// Add a token to the peek-ahead queue.
    /// </summary>
    internal void AddPeekAheadToken(Token token)
    {
        if (_peekAheadIndex < _peekAheadTokens.Count)
            _peekAheadTokens.Insert(_peekAheadIndex, token);
        else
            _peekAheadTokens.Add(token);
        ++_peekAheadIndex;
    }

    /// <summary>
    /// Peek at the next token, and return the text (avoids a null ref check).
    /// </summary>
    public string PeekNextTokenText()
    {
        Token token = PeekNextToken();
        return (token != null ? token.Text : null);
    }

    /// <summary>
    /// Get the next token, skipping comments (which are attached to the last token).
    /// </summary>
    public Token NextToken()
    {
        return NextToken(false);
    }

    /// <summary>
    /// Get the next token, optionally including comments.
    /// </summary>
    /// <param name="includeComments">True if comments shouldn't be skipped.</param>
    public Token NextToken(bool includeComments)
    {
        // Get the next token, using peek-ahead tokens first if we have any
        if (_peekAheadTokens.Count > 0)
        {
            Token = _peekAheadTokens[0];
            _peekAheadTokens.RemoveAt(0);
            if (_peekAheadIndex > 0)
                --_peekAheadIndex;
        }
        else
            Token = ParseNextToken();

        // Move past any trailing comments (unless we're including them)
        if (!includeComments && TokenType == TokenType.Comment)
            ProcessTrailingComments();

        return _token;
    }

    protected void ProcessTrailingComments()
    {
        // Preserve the last token (in case we move past any comments)
        Token lastToken = LastToken;

        // Add each comment to the trailing list of the last token until we're done
        do
        {
            // Create a dummy token if no last token (top of file)
            if (lastToken == null)
            {
#if DEBUG
                lastToken = new Token(null, TokenType.None, false, false, 1, 1, 1, "", CodeUnit);
#else
                lastToken = new Token(null, TokenType.None, false, false, 1, 1, 1, "");
#endif
            }
            if (lastToken.TrailingComments == null)
                lastToken.TrailingComments = new List<CommentBase>();
            CommentBase comment = (CommentBase)ProcessToken(null);
            if (comment != null)
                lastToken.TrailingComments.Add(comment);
        }
        while (TokenType == TokenType.Comment);

        // Set the last token, ingoring any skipped comments
        LastToken = lastToken;
    }

    /// <summary>
    /// Get the next token on the same line (including comments).  Returns null if none.
    /// </summary>
    public Token NextTokenSameLine(bool includeComments)
    {
        Token next = NextToken(includeComments);
        if (next == null)
            return null;
        if (next.IsFirstOnLine)
        {
            // If we were including comments, but the comment was on the following
            // line, we have to move past it and any following comments.
            if (TokenType == TokenType.Comment)
                ProcessTrailingComments();
            return null;
        }
        return next;
    }

    /// <summary>
    /// Get the current token if it's an identifier, otherwise return null; also, advance to the next token.
    /// </summary>
    public Token GetIdentifier()
    {
        Token token = null;
        if (_token != null && _token.IsIdentifier)
        {
            token = _token;
            NextToken();  // Move past token
        }
        return token;
    }

    /// <summary>
    /// Get the text of the current token if it's an identifier, otherwise return null; also, advance to the next token.
    /// Does NOT return the '@' verbatim prefix if there was one.
    /// </summary>
    public string GetIdentifierText()
    {
        Token identifier = GetIdentifier();
        return (identifier != null ? identifier.NonVerbatimText : null);
    }

    /// <summary>
    /// Move any non-EOL comments we just passed to the unused list.
    /// </summary>
    public void MoveCommentsToUnused()
    {
        if (LastToken != null && LastToken.TrailingComments != null)
        {
            foreach (CommentBase commentBase in LastToken.TrailingComments)
                Unused.Add(new UnusedCodeObject(commentBase, LastToken));
            LastToken.TrailingComments = null;
        }
    }

    /// <summary>
    /// Move any non-EOL comments we just passed to the Post unused list.
    /// </summary>
    public void MoveCommentsToPostUnused()
    {
        if (LastToken != null && LastToken.TrailingComments != null)
        {
            if (PostUnused == null)
                PostUnused = new List<ParsedObject>();
            foreach (CommentBase commentBase in LastToken.TrailingComments)
                PostUnused.Add(new UnusedCodeObject(commentBase, LastToken));
            LastToken.TrailingComments = null;
        }
    }

    /// <summary>
    /// Get the OperatorInfo structure for the current token.
    /// </summary>
    public OperatorInfo GetOperatorInfoForToken()
    {
        string token = Token.Text;
        if (!HasUnusedExpression)
            token += "U";  // Differentiate unary operators
        _operatorInfoMap.TryGetValue(token, out OperatorInfo operatorInfo);
        return operatorInfo;
    }

    // Parse the next available 'token' from the input stream, ignoring whitespace.
    protected Token ParseNextToken()
    {
        // Identifiers: \@?[_A-Za-z][A-Za-z0-9]*   (includes keywords, which are checked later)
        // Comments: // or /* ... */
        // String Literal: \@?".*"   (verbatim strings can include linefeeds)
        // Char Literal: '.'
        // Numeric Literal: \.?[0-9].*   (further validation is done later)
        // Symbols: any other chars, but grouped into valid symbols, such as "." or "+="

        // Skip leading whitespace, also treating a Null char or UTF-8 header char as whitespace
        _start = _pos;
        _previous = "";
        while ((char.IsWhiteSpace(_ch) || _ch == '\x0' || _ch == '\xfeff') && !_docCommentCodeContentTerminationDetected)
        {
            NextChar();
            if (_line == null) break;
        }

        // Handle end-of-stream
        if (_line == null)
            return null;

        // Save leading whitespace to be stored on the token
        string whitespace = _line.Substring(_start, _pos - _start);

        // Mark start of token
        _start = _pos;
        int tokenStart = _start;  // Preserve start in case there are multiple lines (such as a block comment)
        _previous = "";
        int tokenStartLine = LineNumber;
        int newLines = LineNumber - _startLine;
        bool skipSpace = false;
        bool leaveNewLine = false;
        bool wasEscaped = _wasEscaped;
        bool hasUnicodeEscapeSequence = false;
        TokenType tokenType;

        if (_docCommentCodeContentTerminationDetected)
        {
            _inDocCommentCodeContent = false;
            // Fake end-of-stream if doc comment content has terminated, until the flag is reset.
            // Note that any peek-ahead tokens will still be flushed first.
            return null;
        }

        // Handle parsing of documentation comments
        if (_inDocComment && !_inDocCommentCodeContent)
        {
            if (_inDocCommentTag)
            {
                // Handle the content of a CDATA tag
                if (_inDocCommentCDATA)
                {
                    // Handle CDATA content
                    while (_inDocComment && !(_ch == ']' && _pk == ']' && _line[_pos + 2] == '>') && !_docCommentCodeContentTerminationDetected)
                        NextChar();
                    if (_startLine == LineNumber && _pos == _start)
                    {
                        // Handle a CDATA terminator
                        tokenType = TokenType.DocCommentSymbol;
                        _pos += 2;
                        NextChar();
                        _inDocCommentTag = _inDocCommentCDATA = false;
                    }
                    else
                    {
                        // Leave one newline for the next token after the doc comment
                        if (!_inDocComment)
                            leaveNewLine = true;
                        tokenType = TokenType.DocCommentString;
                    }
                }
                // Handle the start of a CDATA tag
                else if (_ch == '!' && _line.Substring(_pos).StartsWith(DocCDATA.ParseToken))
                {
                    _pos += DocCDATA.ParseToken.Length - 1;
                    NextChar();
                    tokenType = TokenType.DocCommentTag;
                    _inDocCommentCDATA = true;
                }
                // Handle tag or attribute name
                else if (char.IsLetter(_ch) || _ch == '_')
                {
                    do NextChar();
                    while ((char.IsLetterOrDigit(_ch) || _ch == '_' || _ch == '-' || _ch == '.' || _ch == ':') && _inDocComment);
                    tokenType = TokenType.DocCommentTag;
                }
                // Handle end-of-tag
                else if (_ch == '>' && !_wasEscaped)
                {
                    _inDocCommentTag = false;
                    NextChar();
                    tokenType = TokenType.DocCommentSymbol;
                }
                // Handle other symbols (primarily '=', '/', '"', and '\'', but also any others)
                else
                {
                    NextChar();
                    tokenType = TokenType.DocCommentSymbol;
                }
            }
            else
            {
                // Handle start-of-tag
                if (_ch == '<' && !_wasEscaped)
                {
                    _inDocCommentTag = true;
                    NextChar();
                    tokenType = TokenType.DocCommentSymbol;
                }
                // Handle end-of-block
                else if (_inBlockDocComment && _ch == '*' && _pk == '/')
                {
                    // Skip the token, turn off doc comment mode, and advance to the next token
                    NextChar();
                    NextChar();
                    _startLine = LineNumber;
                    _start = _pos;
                    if (_inDocCommentCodeContent)
                        _docCommentCodeContentTerminationDetected = true;
                    _inDocComment = _inBlockDocComment = false;
                    return ParseNextToken();
                }
                // Handle comment text (outside of any tags)
                else
                {
                    while (_inDocComment && !(_ch == '<' && !_wasEscaped)
                        && !(_inBlockDocComment && _ch == '*' && _pk == '/') && !_docCommentCodeContentTerminationDetected)
                        NextChar();
                    // Leave one newline for the next token after the doc comment
                    if (!_inDocComment)
                        leaveNewLine = true;
                    tokenType = TokenType.DocCommentString;
                }
            }
        }
        // Handle normal code parsing
        else
        {
            // Handle identifier or keyword, including embedded unicode escape sequences.
            // Also treat any Unicode chars as part of an identifier (since they can't be any of the valid symbols checked later below).
            if (char.IsLetter(_ch) || _ch > '\xff' || _ch == '_' || (_ch == '@' && (char.IsLetter(_pk) || _pk == '_')) || (_ch == '\\' && (_pk == 'u' || _pk == 'U')))
            {
                do
                {
                    if (_ch == '\\' && (_pk == 'u' || _pk == 'U'))
                        hasUnicodeEscapeSequence = true;
                    NextChar();
                }
                while (char.IsLetterOrDigit(_ch) || _ch > '\xff' || _ch == '_' || (_ch == '\\' && (_pk == 'u' || _pk == 'U')));
                tokenType = TokenType.Identifier;
            }
            // Handle single-line EOL comment or documentation (XML) comment
            else if (_ch == '/' && _pk == '/')
            {
                NextChar();
                NextChar();
                if (_ch == '/' && _pk != '/')
                {
                    // Handle documentation comment
                    NextChar();
                    tokenType = TokenType.DocCommentStart;
                    _inDocComment = true;
                    skipSpace = true;
                }
                else
                {
                    // Jump ahead to EOL
                    _pos = _length - 2;
                    NextChar();
                    tokenType = TokenType.Comment;
                }
            }
            // Handle block comment or block-style documentation (XML) comment
            else if (_ch == '/' && _pk == '*')
            {
                NextChar();
                NextChar();
                if (_ch == '*' && _pk != '*' && _pk != '/')
                {
                    NextChar();
                    tokenType = TokenType.DocCommentStart;
                    _inDocComment = _inBlockDocComment = true;
                    _blockDocCommentPrefix = null;  // Will get set on 2nd line of block comment
                    skipSpace = true;
                }
                else
                {
                    while (!(_ch == '*' && _pk == '/') && _line != null && !_docCommentCodeContentTerminationDetected)
                        NextChar();
                    NextChar();
                    NextChar();
                    tokenType = TokenType.Comment;
                }
            }
            // Handle verbatim string literal
            else if (_ch == '@' && _pk == '"')
            {
                NextChar();
                do
                {
                    NextChar();
                    while (_ch == '"' && _pk == '"')
                    {
                        NextChar();
                        NextChar();
                    }
                }
                while (_ch != '"' && _line != null && !_docCommentCodeContentTerminationDetected);
                NextChar();
                tokenType = TokenType.VerbatimString;
            }
            // Handle string literal
            else if (_ch == '"')
            {
                do
                {
                    NextChar();
                    while (_ch == '\\')
                    {
                        NextChar();
                        NextChar();
                    }
                }
                while (_ch != '"' && _line != null && !_docCommentCodeContentTerminationDetected);
                NextChar();
                tokenType = TokenType.String;
            }
            // Handle char literal
            else if (_ch == '\'')
            {
                do
                {
                    NextChar();
                    while (_ch == '\\')
                    {
                        NextChar();
                        NextChar();
                    }
                }
                while (_ch != '\'' && _ch != '\n' && _line != null && !_docCommentCodeContentTerminationDetected);
                NextChar();
                tokenType = TokenType.Char;
            }
            // Handle numeric literal
            else if (char.IsDigit(_ch) || (_ch == '.' && char.IsDigit(_pk)))
            {
                bool gotDot = false;
                bool isHex = false;

                // Handle hex literal
                if (_ch == '0')
                    NextChar();
                if ((_ch == 'x' || _ch == 'X') && Uri.IsHexDigit(_pk))
                {
                    do
                        NextChar();
                    while (Uri.IsHexDigit(_ch));
                    isHex = true;
                }
                else
                {
                    // Handle remaining numerics, including fractions and exponents
                    while (char.IsDigit(_ch) || (!gotDot && _ch == '.' && char.IsDigit(_pk)))
                    {
                        gotDot = (_ch == '.');
                        NextChar();
                    }

                    // Handle exponent
                    char pk2 = ((_pos < _length - 2) ? _line[_pos + 2] : '\0');
                    if ((_ch == 'e' || _ch == 'E') && (char.IsDigit(_pk) || ((_pk == '-' || _pk == '+') && char.IsDigit(pk2))))
                    {
                        NextChar();
                        if (_ch == '-' || _ch == '+') NextChar();
                        while (char.IsDigit(_ch)) NextChar();
                    }
                }

                // Handle suffixes
                if (!gotDot && (_ch == 'u' || _ch == 'U'))
                {
                    NextChar();
                    if (_ch == 'l' || _ch == 'L') NextChar();
                }
                else if (!gotDot && (_ch == 'l' || _ch == 'L'))
                {
                    NextChar();
                    if (_ch == 'u' || _ch == 'U') NextChar();
                }
                else if (!isHex && "fFdDmM".IndexOf(_ch) >= 0)
                    NextChar();

                tokenType = TokenType.Numeric;
            }
            // Handle compiler directives
            else if (_ch == '#')
            {
                NextChar();
                tokenType = TokenType.CompilerDirective;
            }
            // Handle symbols
            else
            {
                _symbolMap.TryGetValue(_ch, out CharMap map);
                NextChar();

                // For tokens starting with a symbol, extract the longest possible token
                // according to the pre-generated mapping dictionaries.
                while (map != null && map.TryGetValue(_ch, out map))
                    NextChar();

                tokenType = TokenType.Symbol;
            }
        }

        // Extract and return the token
        Token token = null;
        if (_line != null)
        {
#if DEBUG
            token = new Token(_previous + _line.Substring(_start, _pos - _start), tokenType, wasEscaped, _inDocComment,
                tokenStartLine, tokenStart + 1, newLines, whitespace, CodeUnit);
#else
            token = new Token(_previous + _line.Substring(_start, _pos - _start), tokenType, wasEscaped, _inDocComment,
                tokenStartLine, tokenStart + 1, newLines, whitespace);
#endif
            if (skipSpace && _ch == ' ')
                NextChar();

            // Count any lines with at least one identifier towards the SLOC total
            if (token.TokenType == TokenType.Identifier)
                _isSLOC = true;

            _startLine = LineNumber;
            if (leaveNewLine)
                --_startLine;

            // Convert any unicode escape sequences embedded in identifiers into the actual unicode characters.
            // VS 2010 leaves them alone (for editing), but the compiler converts them.  We convert them on parsing
            // so that they can be displayed as actual Unicode chars (which is really the intent), and so that string
            // matching will work properly.  Editing them or searching for them will be handled by the GUI, and they
            // will be converted back to escape sequences if saved as text.
            if (hasUnicodeEscapeSequence)
                token.Text = StringUtil.ConvertUnicodeEscapes(token.Text);
        }

        _start = _pos;
        return token;
    }

    // Get the next character from the input stream.
    protected internal void NextChar()
    {
        // Don't allow advancement until the termination flag is cleared
        if (_docCommentCodeContentTerminationDetected)
            return;

        // Increment position and check for EOL
        if (++_pos >= _length)
        {
            // Save any leftover data on the current line for the token being parsed
            if (_line != null)
                _previous += _line.Substring(_start, _length - _start);

            // Count the last line towards the SLOC total if it contained any SLOC tokens
            if (_isSLOC)
            {
                ++SLOC;
                _isSLOC = false;
            }

            // Read the next line
            _start = _pos = 0;
            _line = _textReader.ReadLine();
            if (_line == null)
            {
                _length = 0;
                _ch = _pk = '\0';
                if (_inDocCommentCodeContent)
                    _docCommentCodeContentTerminationDetected = true;
                _inDocComment = _inBlockDocComment = _inDocCommentTag = _inDocCommentCodeContent = _inDocCommentCDATA = false;
                return;
            }
            _line += '\n';
            ++LineNumber;

            if (_line.Length > 0)
            {
                if (_line[0] == ' ')
                    ++_linesWithSpaces;
                else if (_line[0] == '\t')
                {
                    ++_linesWithTabs;

                    // Expand any tabs used for indentation into spaces.  Tabs between tokens can be left alone, and should
                    // be stripped away during parsing.  Tabs in comments are left alone on purpose, in case they're wanted.
                    StringBuilder expanded = new(_line.Length * 2);
                    for (int i = 0; i < _line.Length; ++i)
                    {
                        if (_line[i] == '\t')
                            expanded.Append(' ', CodeObject.TabSize - (expanded.Length % CodeObject.TabSize));
                        else if (_line[i] == ' ')
                            expanded.Append(' ');
                        else
                        {
                            expanded.Append(_line.Substring(i));
                            break;
                        }
                    }
                    _line = expanded.ToString();
                }
            }

            // Get the length of the new line
            _length = _line.Length;

            // Handle newlines in documentation comments (skip over the line prefix)
            if (_inDocComment)
            {
                // Force termination of code content if looking for a single char delimiter and we hit a newline
                if (_inDocCommentCodeContent && _docCommentCodeContentTerminator.Length == 1)
                    _docCommentCodeContentTerminationDetected = true;

                if (_inBlockDocComment)
                {
                    if (_blockDocCommentPrefix != null)
                    {
                        // Skip the expected whitespace-asterisk-whitespace prefix, if found and the asterisk isn't followed by a '/'
                        int prefixLength = _blockDocCommentPrefix.Length;
                        if (prefixLength > 0 && string.CompareOrdinal(_line, 0, _blockDocCommentPrefix, 0, prefixLength) == 0
                            && !(_blockDocCommentPrefix[prefixLength - 1] == '*' && _length > prefixLength && _line[prefixLength] == '/'))
                            _start = _pos = _blockDocCommentPrefix.Length;
                        else
                        {
                            // Otherwise, at least skip the leading whitespace if possible
                            int whitespace = StringUtil.CharCount(_line, ' ', 0);
                            if (whitespace <= _blockDocWhitespacePrefix)
                                _start = _pos = whitespace;
                        }
                    }
                    else
                    {
                        // Skip and remember the whitespace-asterisk-whitespace prefix pattern (if any)
                        while (_pos < _length && _line[_pos] == ' ') ++_pos;
                        if (_pos < _length && _line[_pos] == '*' && !(_pos < _length - 1 && _line[_pos + 1] == '/')) ++_pos;
                        while (_pos < _length && _line[_pos] == ' ') ++_pos;
                        _blockDocCommentPrefix = _line.Substring(0, _pos);
                        _blockDocWhitespacePrefix = StringUtil.CharCount(_line, ' ', 0);
                        _start = _pos;
                    }
                }
                else
                {
                    // Skip the expected '///' prefix (and optional trailing space), or we're done if it's not there
                    int pos = _pos;
                    // First, skip any preceeding spaces
                    while (pos < _length && _line[pos] == ' ')
                        ++pos;
                    // Verify that we have '///' and that it's not followed by another '/' (or it's a regular '//' comment)
                    if (pos <= _length - 3 && _line[pos] == '/' && _line[pos + 1] == '/' && _line[pos + 2] == '/' && (pos == _length - 3 || _line[pos + 3] != '/'))
                    {
                        _pos = pos + 3;
                        if (_pos < _length && _line[_pos] == ' ') ++_pos;
                        _start = _pos;
                    }
                    else
                    {
                        // Get rid of any trailing blank line inside a doc comment
                        if (_previous == "\n\n")
                            ++_startLine;
                        // Force termination of code content if we're no longer in a doc comment
                        if (_inDocCommentCodeContent)
                            _docCommentCodeContentTerminationDetected = true;
                        _inDocComment = _inBlockDocComment = _inDocCommentTag = _inDocCommentCodeContent = _inDocCommentCDATA = false;
                    }
                }
            }
        }

        // Read current and next chars
        _ch = _line[_pos];
        _pk = ((_pos < _length - 1) ? _line[_pos + 1] : '\0');
        _wasEscaped = false;

        // Check for termination of embedded code parsing mode
        if (_inDocCommentCodeContent)
        {
            int terminatorLength = _docCommentCodeContentTerminator.Length;
            if (terminatorLength == 1)
            {
                // Handle single-char terminator or end-of-tag
                if (_ch == _docCommentCodeContentTerminator[0] || (_ch == '/' && _pk == '>'))
                    _docCommentCodeContentTerminationDetected = true;
            }
            else if (_ch == _docCommentCodeContentTerminator[0])
            {
                // Terminate only if entire terminator exists on the current line
                if (_line.Length - _pos >= terminatorLength && _line.Substring(_pos, terminatorLength) == _docCommentCodeContentTerminator)
                    _docCommentCodeContentTerminationDetected = true;
            }
        }

        // Check for escaped chars inside documentation comments
        if (_inDocComment && _ch == '&')
        {
            switch (_pk)
            {
                case 'l': TranslateEscapedChar("lt", '<'); break;
                case 'g': TranslateEscapedChar("gt", '>'); break;
                case 'a': TranslateEscapedChar("amp", '&'); break;
            }
        }
    }

    private void TranslateEscapedChar(string sequence, char result)
    {
        int pos = _pos + 1;
        int offset = 0;
        while (pos < _length && offset < sequence.Length && _line[pos] == sequence[offset])
        {
            ++offset;
            ++pos;
        }
        if (offset == sequence.Length && pos < _length - 1 && _line[pos] == ';')
        {
            _ch = result;
            _line = _line.Substring(0, _pos) + _ch + _line.Substring(pos + 1);
            _length = _line.Length;
            // Fixup peek-ahead char
            _pk = ((_pos < _length - 1) ? _line[_pos + 1] : '\0');
            _wasEscaped = true;
        }
    }

    /// <summary>
    /// Get the current line of text including the EOL character, and advance to the next token, including comments.
    /// </summary>
    public string GetCurrentLine()
    {
        // Force DocComment mode off so we can read line-by-line
        _inDocComment = false;
        _peekAheadTokens.Clear();
        _peekAheadIndex = 0;
        string result = _line;
        _start = _pos = _length;
        NextChar();
        NextToken(true);
        return result;
    }

    /// <summary>
    /// Parse from the current token up to (but not including) the EOL, returning that as a string, and advancing to the next token.
    /// </summary>
    public string GetTokenToEOL()
    {
        _start = _pos;
        _pos = _length - 1;  // Stop just short of the EOL
        string result = TokenText + _line.Substring(_start, _pos - _start);
        NextChar();
        NextToken();
        return result;
    }

    /// <summary>
    /// Parse from just after the current token up to (but not including) the specified delimiter, returning that as a string, and
    /// advancing to the delimiter as the next token.  Stops just short of EOL if the delimiter isn't found.
    /// </summary>
    public string GetToDelimiter(char delimiter)
    {
        _start = _pos;
        _pos = _line.IndexOf(delimiter, _start);
        if (_pos < 0)
            _pos = _length - 1;  // Stop just short of the EOL
        string result = _line.Substring(_start, _pos - _start);
        --_pos;
        NextChar();
        NextToken();
        return result;
    }

    /// <summary>
    /// Determine if the current token is indented less than the specified starting token.
    /// </summary>
    public bool CurrentTokenIndentedLessThan(Token startingToken)
    {
        return (_token != null && _token.LineNumber > startingToken.LineNumber && _token.ColumnNumber < startingToken.ColumnNumber);
    }

    /// <summary>
    /// Returns true if more lines in the parsed file start with tabs than spaces.
    /// </summary>
    public bool UsingMoreTabsThanSpaces()
    {
        return (_linesWithSpaces < _linesWithTabs);
    }

    public void Dispose()
    {
        if (_textReader != null)
        {
            _textReader.Close();
            _textReader = null;
        }
    }

    /// <summary>
    /// Create a parser error message in connection with the specified <see cref="Token"/> and attach it to the related <see cref="CodeObject"/>.
    /// </summary>
    public void AttachMessage(CodeObject codeObject, string text, Token token)
    {
        // The token can be null because of EOF or the end of embedded code in a doc comment - in this case, create
        // a dummy token (which provides line/column information and InDocComment state).
        if (token == null)
#if DEBUG
            token = new Token(null, TokenType.None, false, _inDocComment, LineNumber, _start + 1, 0, null, CodeUnit);
#else
            token = new Token(null, TokenType.None, false, _inDocComment, LineNumber, _start + 1, 0, null);
#endif
        codeObject.AttachAnnotation(new TokenMessage(text, token));
    }

    /// <summary>
    /// True if parsing a compiler directive expression.
    /// </summary>
    public bool InDirectiveExpression;

    // Stores the current state of nested conditional directives
    private readonly Stack<bool> _conditionalDirectiveStack = new();

    /// <summary>
    /// Get the current conditional directive state.
    /// </summary>
    public bool CurrentConditionalDirectiveState
    {
        get { return _conditionalDirectiveStack.Peek(); }
        set
        {
            _conditionalDirectiveStack.Pop();
            _conditionalDirectiveStack.Push(value);
        }
    }

    /// <summary>
    /// Indicate the start of a conditional directive.
    /// </summary>
    public void StartConditionalDirective()
    {
        _conditionalDirectiveStack.Push(false);
    }

    /// <summary>
    /// Indicate the end of a conditional directive.
    /// </summary>
    public void EndConditionalDirective()
    {
        _conditionalDirectiveStack.Pop();
    }

    // Map of CompilerDirective tokens to ParseDelegate callbacks
    private static readonly Dictionary<string, ParseDelegate> _compilerDirectiveMap = new();

    /// <summary>
    /// Add a parse-point for a compiler directive - triggers the callback when the specified token appears after the '#'.
    /// </summary>
    public static void AddCompilerDirectiveParsePoint(string token, ParseDelegate callback)
    {
        _compilerDirectiveMap.Add(token, callback);
    }

    private bool _inDocComment;
    private bool _inBlockDocComment;
    private bool _inDocCommentTag;
    private bool _inDocCommentCodeContent;
    private bool _inDocCommentCDATA;

    private int _blockDocWhitespacePrefix;
    private string _blockDocCommentPrefix;
    private string _docCommentCodeContentTerminator;
    private bool _docCommentCodeContentTerminationDetected;

    /// <summary>
    /// True if parsing a documentation comment.
    /// </summary>
    public bool InDocComment { get { return _inDocComment; } }

    /// <summary>
    /// Parse an embedded code expression inside a documentation comment, starting with the current token, and parsing
    /// up to the specified delimiter.
    /// </summary>
    public Expression ParseCodeExpressionUntil(string delimiter, CodeObject parent)
    {
        // Change parsing mode to embedded code, and set the delimiter that will switch it back
        _inDocCommentCodeContent = true;
        _docCommentCodeContentTerminator = delimiter;
        NextToken();  // Move past starting delimiter

        // Parse the embedded code expression
        Expression expression = Expression.Parse(this, parent, true);

        // Clear the special termination mode, and fix the Token variables if necessary
        bool stillInContent = _inDocCommentCodeContent;
        _inDocCommentCodeContent = _docCommentCodeContentTerminationDetected = false;
        if (_token == null)
            NextToken();

        // Handle any extraneous characters as unrecognized text
        // (this can occur if any of ",;}" are encountered before the delimiter)
        if (stillInContent && !delimiter.StartsWith(TokenText))
        {
            // Just ignore a single trailing ';' - these might be used like '<c>Method();</c>'
            if (TokenText == Statement.ParseTokenTerminator)
                NextToken();
            else
            {
                string badText = Token.LeadingWhitespace + TokenText + GetToDelimiter(delimiter[0]);
                Unrecognized unrecognized = new(false, _inDocComment, expression, new UnresolvedRef(badText, Token));
                unrecognized.UpdateMessage();
                expression = unrecognized;
            }
        }

        return expression;
    }

    /// <summary>
    /// Parse an embedded code block inside a documentation comment, starting with the current token, and parsing
    /// up to the specified delimiter.
    /// </summary>
    public CodeObject ParseCodeBlockUntil(string delimiter, CodeObject parent)
    {
        CodeObject result = null;

        // Change parsing mode to embedded code, and set the delimiter that will switch it back
        _inDocCommentCodeContent = true;
        _docCommentCodeContentTerminator = delimiter;
        NextToken();  // Move past starting delimiter

        // Abort parsing as code if the first token looks like a doc comment start tag
        if (TokenText == DocComment.ParseTokenTagOpen)
        {
            // Force the token into what it would be if not code
            _inDocCommentTag = true;
            TokenType = TokenType.DocCommentSymbol;
        }
        else
        {
            // Parse the embedded code block, or single identifier
            if (!_docCommentCodeContentTerminationDetected)
            {
                new Block(out Block body, this, parent, false, delimiter);
                result = body;

                // Flush any remaining unused objects (just in case)
                body.FlushUnused(this);
            }
            else if (Token != null)
            {
                result = ProcessToken(parent, ParseFlags.Expression);
                if (result == null)
                {
                    if (HasUnusedExpression)
                        result = RemoveLastUnusedExpression(true);
                    else if (HasUnused)
                        result = new UnresolvedRef(RemoveLastUnusedToken());
                }
            }
        }

        // Clear the special termination mode, and fix the Token variables
        _inDocCommentCodeContent = _docCommentCodeContentTerminationDetected = false;
        if (_token == null)
            NextToken();

        return result;
    }

    // Map of DocComment tags to ParseDelegate callbacks
    private static readonly Dictionary<string, ParseDelegate> _docCommentTagMap = new();

    /// <summary>
    /// Add a documentation comment tag for callback during parsing.
    /// </summary>
    public static void AddDocCommentParseTag(string tag, ParseDelegate callback)
    {
        _docCommentTagMap.Add(tag, callback);
    }

    /// <summary>
    /// Delegate for parser callbacks.
    /// </summary>
    public delegate CodeObject ParseDelegate(Parser parser, CodeObject parent, ParseFlags flags);

    // Map of tokens to parse-point information
    private static readonly Dictionary<string, List<ParsePoint>> _tokenMap = new();

    /// <summary>
    /// Add a parse-point to the parser - the callback is triggered when the token appears in the specified context.
    /// </summary>
    public static void AddParsePoint(string token, int priority, ParseDelegate callback, params Type[] contextTypes)
    {
        ParsePoint parsePoint = new() { ContextTypes = contextTypes, Priority = priority, Callback = callback };
        if (_tokenMap.TryGetValue(token, out List<ParsePoint> list))
        {
            // Add the parse-point to the existing list, sorted by priority
            for (int i = 0; i < list.Count; ++i)
            {
                ParsePoint current = list[i];
                if (current.Priority > priority)
                {
                    list.Insert(i, parsePoint);
                    break;
                }
                if (current.Priority == priority)
                    throw new Exception("Duplicate parse-point added!  Needs different priority!");

                // If we reached the end of the list, add the new parse-point at the end
                if (i == list.Count - 1)
                {
                    list.Add(parsePoint);
                    break;
                }
            }
        }
        else
            _tokenMap.Add(token, new List<ParsePoint> { parsePoint });

        // Add any symbols longer than a single letter to the tokenizing maps
        if (!char.IsLetterOrDigit(token[0]) && token.Length > 1)
            _symbolMap.AddSymbolMap(token);
    }

    /// <summary>
    /// Add a parse-point to the parser - the callback is triggered when the token appears in the specified context.
    /// </summary>
    public static void AddParsePoint(string token, ParseDelegate callback, params Type[] contextTypes)
    {
        AddParsePoint(token, 0, callback, contextTypes);
    }

    /// <summary>
    /// Add a parse-point to the parser - the callback is triggered when the token is found.
    /// </summary>
    public static void AddParsePoint(string token, int priority, ParseDelegate callback)
    {
        AddParsePoint(token, priority, callback, null);
    }

    /// <summary>
    /// Add a parse-point to the parser - the callback is triggered when the token is found.
    /// </summary>
    public static void AddParsePoint(string token, ParseDelegate callback)
    {
        AddParsePoint(token, 0, callback, null);
    }

    /// <summary>
    /// Add a parse-point to the parser for an operator - the callback is triggered when the token appears in the specified context.
    /// </summary>
    public static void AddOperatorParsePoint(string token, int priority, int precedence, bool leftAssociative, bool isUnary, ParseDelegate callback)
    {
        AddOperatorInfo(token, precedence, leftAssociative, isUnary);

        // No restriction is put on the context for operators, since they can appear in most Expressions and many Statements
        AddParsePoint(token, priority, callback, null);
    }

    /// <summary>
    /// Add a parse-point to the parser for an operator - the callback is triggered when the token appears in the specified context.
    /// </summary>
    public static void AddOperatorParsePoint(string token, int precedence, bool leftAssociative, bool isUnary, ParseDelegate callback)
    {
        AddOperatorParsePoint(token, 0, precedence, leftAssociative, isUnary, callback);
    }

    private static void AddOperatorInfo(string token, int precedence, bool leftAssociative, bool unary)
    {
        if (unary)
            token += "U";  // Differentiate unary operators
        _operatorInfoMap.Add(token, new OperatorInfo(precedence, leftAssociative, unary));
    }

    private class ParsePoint
    {
        public int Priority;
        public ParseDelegate Callback;
        public Type[] ContextTypes;

        /// <summary>
        /// Determine if the parse-point is valid for the specified parent object.
        /// </summary>
        public bool IsContextValid(Parser parser, CodeObject parent, ParseFlags flags)
        {
            bool validContext = true;

            // Check if we have context restrictions
            if (ContextTypes != null && ContextTypes.Length > 0 && parent != null)
            {
                // Relax restrictions if code is embedded in a doc comment
                bool relaxedMode = parser.InDocComment;
                validContext = false;

                // Only parse for the specified valid parent types
                Type parentType = parent.GetType();
                foreach (Type type in ContextTypes)
                {
                    // Allow IBlock, TypeDecl, and NamespaceDecl restrictions to always work in relaxed mode,
                    // otherwise we have a match if the type is the same or a base type of the one we're checking.
                    if ((relaxedMode && (type == typeof(IBlock) || type == typeof(TypeDecl) || type == typeof(NamespaceDecl)))
                        || type.IsAssignableFrom(parentType))
                    {
                        validContext = true;
                        break;
                    }
                }
            }

            // If we're parsing an Expression, we want to disallow all Statements
            if (flags.HasFlag(ParseFlags.Expression) && typeof(Statement).IsAssignableFrom(Callback.Method.DeclaringType))
                validContext = false;

            return validContext;
        }
    }

    // Nested maps of symbols used to tokenize the longest possible tokens
    private static readonly CharMap _symbolMap = new();

    private class CharMap : Dictionary<char, CharMap>
    {
        public void AddSymbolMap(string token)
        {
            char ch = token[0];
            if (!TryGetValue(ch, out CharMap map))
            {
                map = new CharMap();
                Add(ch, map);
            }
            if (token.Length > 1)
                map.AddSymbolMap(token.Substring(1));
        }
    }

    // Map of operators to operator information
    private static readonly Dictionary<string, OperatorInfo> _operatorInfoMap = new();

    /// <summary>
    /// Contains information about an operator.
    /// </summary>
    public class OperatorInfo
    {
        /// <summary>
        /// The precedence of the operator.
        /// </summary>
        public int Precedence;

        /// <summary>
        /// True if the operator is left-associative (otherwise it's right-associative).
        /// </summary>
        public bool LeftAssociative;

        /// <summary>
        /// True if the operator is unary.
        /// </summary>
        public bool Unary;

        /// <summary>
        /// Create an <see cref="OperatorInfo"/> instance.
        /// </summary>
        public OperatorInfo(int precedence, bool leftAssociative, bool unary)
        {
            Precedence = precedence;
            LeftAssociative = leftAssociative;
            Unary = unary;
        }
    }
}