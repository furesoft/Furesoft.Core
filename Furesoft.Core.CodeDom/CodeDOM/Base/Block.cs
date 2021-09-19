using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Furesoft.Core.CodeDom.Utilities;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.DocComments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals;
using Attribute = Furesoft.Core.CodeDom.CodeDOM.Annotations.Attribute;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Other;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Namespaces;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Types.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Base
{
    /// <summary>
    /// Represents the body of a <see cref="BlockStatement"/> or <see cref="AnonymousMethod"/> (containing
    /// a sequence of 0 or more <see cref="CodeObject"/>s).
    /// </summary>
    /// <remarks>
    /// Blocks are special in that their children's Parent references link directly to the parent of the Block, in order to simplify various logic.
    /// </remarks>
    public class Block : CodeObject, ICollection<CodeObject>, ICollection
    {
        /// <summary>
        /// The token used to parse the end of a <see cref="Block"/>.
        /// </summary>
        public const string ParseTokenEnd = "}";

        /// <summary>
        /// The token used to parse the start of a <see cref="Block"/>.
        /// </summary>
        public const string ParseTokenStart = "{";

        /// <summary>
        /// Child <see cref="CodeObject"/>s - the Parent of this collection will be the Block's Parent, so
        /// that the Parent of all child objects will be the Block's Parent, not the Block.
        /// </summary>
        protected ChildList<CodeObject> _codeObjects;

        /// <summary>
        /// Dictionary of named members in the block (LocalDecls, Labels, SwitchItems, or
        /// various other members if the block's parent is a TypeDecl).
        /// </summary>
        protected NamedCodeObjectDictionary _namedMembers;

        /// <summary>
        /// Create a <see cref="Block"/>, optionally with the specified code objects.
        /// </summary>
        public Block(params CodeObject[] codeObjects)
        {
            // The parent will be null at this point, but when it gets set later, all child
            // objects will get their parent set correctly, and we'll also call ReformatBlock().
            _codeObjects = new ChildList<CodeObject>();
            foreach (CodeObject codeObject in codeObjects)
                AddInternal(codeObject);
        }

        /// <summary>
        /// Parse a <see cref="Block"/>.
        /// </summary>
        public Block(out Block parentBody, Parser parser, CodeObject parent, bool bracesRequired, params string[] terminators)
            : base(parser, parent)
        {
            // Clear any newline count, and set IsFirstOnLine appropriately.  Blocks use only 0 or 1 newlines on the '{',
            // with multiple newlines possible on the '}' using the EndNewLines property.
            SetNewLines(0);
            IsFirstOnLine = ((parser.Token != null && parser.Token.IsFirstOnLine) || (parent != null && parent.HasCompilerDirectives));

            // Allocate our list of child objects
            _codeObjects = new ChildList<CodeObject>(parent);

            // Tie our parent to us immediately, so that symbol resolution will work properly
            parentBody = this;

            // Don't process comments or conditional directives if we have no parent, or the parent has no header (CodeUnit or BlockDecl)
            bool processedPostAnnotations = false;
            if (parent is IBlock && ((IBlock)parent).HasHeader)
            {
                // Process comments
                parent.MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment with the parent

                // Add any skipped comment objects to the block object itself if we have an open brace
                if (parser.TokenText == ParseTokenStart)
                    MoveComments(parser.LastToken);
                // Otherwise, add them inside the block as long as it's not empty (like an empty SwitchItem block)
                else if (!StringUtil.Contains(terminators, parser.TokenText))
                    AddTrailingComments(parser.LastToken);

                // Parse any post compiler directives between the parent statement and the block
                processedPostAnnotations = ParseCompilerDirectives(parser, parent, AnnotationFlags.IsPostfix);
            }
            else
            {
                // Add any skipped comments at the top of a CodeUnit or BlockDecl (from the dummy token)
                AddTrailingComments(parser.LastToken);
            }

            // Start a new Unused list in the parser
            parser.PushUnusedList();

            // Check if we have braces - if so, we always parse to the end brace
            bool isTopLevel = (!(_parent is IBlock) || ((IBlock)_parent).IsTopLevel);
            bool singleStatement;
            bool specialTermination;
            bool hasEmptyStatement = false;
            if (parser.TokenText == ParseTokenStart)
            {
                HasBraces = true;
                parser.NextToken();                       // Move past '{'
                MoveEOLCommentAsInfix(parser.LastToken);  // Move any EOL comment as an Infix-EOL
                parser.MoveCommentsToUnused();            // Add skipped Comment objects as unused
                singleStatement = specialTermination = false;
            }
            else
            {
                if (bracesRequired)
                {
                    // If braces are required for this block, but there aren't any, it's a parse error.
                    // Just abort now with a null Block, even though we might possibly have "eaten" some
                    // comments and/or compiler directives (this could be improved).
                    parentBody = null;
                    goto abort;  // Go pop the unused list before exiting
                }

                // If terminators is null, parse to EOF (used by CodeUnit), otherwise assume a single
                // statement block unless specific terminators are provided (such as for SwitchItems).
                singleStatement = (terminators != null && terminators.Length == 0);
                specialTermination = (terminators != null && terminators.Length > 0);

                // Special handling for a block that consists only of a ';'
                if (parser.TokenText == Statement.ParseTokenTerminator)
                {
                    hasEmptyStatement = true;

                    // Skip the token so we can check any trailing comment
                    parser.NextToken();

                    // Move any EOL comment as a normal comment in the Block (a Token can only have one, and it should be the first one)
                    List<CommentBase> comments = parser.LastToken.TrailingComments;
                    if (comments != null && comments.Count > 0)
                    {
                        CommentBase comment = comments[0];
                        if (comment.IsEOL)
                        {
                            comment.IsEOL = false;
                            comment.IsFirstOnLine = true;
                            Add(comment);
                            comments.RemoveAt(0);
                        }
                    }
                    goto abort;  // Go pop the unused list before exiting
                }

                // If we don't have braces, move any trailing non-conditional directive annoations that
                // we moved to the parent above into the body of the block instead.
                if (processedPostAnnotations && !singleStatement)
                {
                    int index = parent.Annotations.Count - 1;
                    do
                    {
                        Annotation annotation = parent.Annotations[index];
                        if (annotation is Comment || (annotation is CompilerDirective && !(annotation is ConditionalDirectiveBase)))
                        {
                            parent.Annotations.RemoveAt(index);
                            InsertInternalNoFormatting(0, annotation);
                        }
                        else
                            break;
                    }
                    while (--index >= 0);
                }
            }

            bool pendingConditionalDirectives = false;

            // Push our parent as an EOL comment normalization blocker
            parser.PushNormalizationBlocker(parent);

            // Loop while not EOF, and we're either delimited by braces or we haven't hit a terminator
            while (parser.Token != null && (HasBraces || !StringUtil.Contains(terminators, parser.TokenText)))
            {
                // Stop if we're at a '}'
                if (parser.TokenText == ParseTokenEnd && !isTopLevel)
                {
                    // Check if we were expecting it
                    if (HasBraces)
                    {
                        EndNewLines = parser.Token.NewLines;  // Set the newline count for the '}'
                        parser.NextToken();                   // Eat the '}'
                        MoveEOLComment(parser.LastToken);
                    }
                    break;
                }

                // Process the current token.
                // Check for obj being null, because it might contain a '#if' directive from the preprocessing above.
                CodeObject obj = parser.ProcessToken(parent, ParseFlags.Block);
                if (obj != null)
                {
                    // If we have a complete statement, or an expression followed by a terminator, add it to the Block
                    if (obj is Statement || (obj is Expression && parser.TokenText == Statement.ParseTokenTerminator))
                    {
                        pendingConditionalDirectives = false;

                        // Eat the terminator after an Expression used as a Statement
                        if (obj is Expression)
                        {
                            obj.HasTerminator = true;
                            parser.NextToken();
                        }

                        // Associate any skipped EOL comment - with the Block if we have a single
                        // statement on the same line, otherwise with the current statement.
                        if (singleStatement && !obj.IsFirstOnLine)
                            MoveEOLComment(parser.LastToken);
                        else
                        {
                            // Move any EOL comment to the statement.
                            // If there's an inline comment between statements in the block (on the
                            // same line), make it an EOL comment on the first statement.
                            obj.MoveEOLComment(parser.LastToken);
                        }

                        bool hasConditionalDirectives = false;
                        if (parser.HasUnused)
                        {
                            hasConditionalDirectives = (parser.LastUnusedCodeObject is ConditionalDirective);
                            FlushUnused(parser);  // Flush any remaining unused objects
                        }

                        // Add the Statement to the Block
                        AddInternalNoFormatting(obj);

                        // Flush any post unused objects (used by IfBase and for trailing comments in specially terminated
                        // blocks below, that are flushed to the parent block).  Skip for single statements so that things
                        // like region directives get pushed up to the parent block of the statement just added.
                        if (parser.PostUnused != null && parser.PostUnused.Count > 0 && !singleStatement)
                        {
                            parser.MovePostUnusedToUnused();
                            FlushUnused(parser);
                        }

                        // Stop if we only wanted a single statement
                        if (singleStatement)
                        {
                            IsFirstOnLine = obj.IsFirstOnLine;

                            // If we had unused conditional directive(s) before the statement, then we
                            // should also include any that come immediately after.
                            if (hasConditionalDirectives)
                                pendingConditionalDirectives = true;
                            else
                                break;
                        }

                        // If we're at the end of a specially terminated block (i.e. case/default block), then move
                        // any trailing comments to the Post unused list for later use, otherwise flush them now.
                        if (specialTermination && (StringUtil.Contains(terminators, parser.TokenText) || parser.TokenText == ParseTokenEnd))
                            parser.MoveCommentsToPostUnused();
                        else
                            AddTrailingComments(parser.LastToken);  // Add skipped Comment objects
                    }
                    else
                    {
                        obj.MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment

                        // Special handling for pending conditional directives
                        if (pendingConditionalDirectives)
                        {
                            if (obj is ConditionalDirectiveBase)
                            {
                                AddInternalNoFormatting(obj);
                                AddTrailingComments(parser.LastToken);  // Add skipped Comment objects
                                if (obj is EndIfDirective)
                                    break;
                                obj = null;
                            }
                            else
                                pendingConditionalDirectives = false;
                        }

                        // Save the object for later use
                        if (obj != null)
                            parser.AddUnused(obj);

                        // Move any trailing comments to the unused list
                        parser.MoveCommentsToUnused();
                    }
                }
            }

            // Pop the normalization blocker object
            parser.PopNormalizationBlocker();

            // Flush any remaining unused objects
            FlushUnused(parser);

            // Do various checks if auto-cleanup is on
            if (AutomaticFormattingCleanup && !parser.IsGenerated)
            {
                if (HasBraces)
                {
                    if (_codeObjects.Count > 0)
                    {
                        // Remove unnecessary braces
                        if (_parent is BlockStatement && !((BlockStatement)_parent).ShouldHaveBraces())
                        {
                            // Also do a special check for 'else' clauses on the same line as the closing '}', changing them to be first-on-line
                            if (EndNewLines > 0 && _parent is IfBase && parser.TokenText == Else.ParseToken && parser.Token.NewLines == 0)
                                parser.Token.NewLines = 1;
                            HasBraces = false;
                        }
                    }
                    else if (_codeObjects.Count == 0)
                    {
                        // Force empty braces onto a single line if empty and the block starts on a new line and it's allowed by default
                        if (EndNewLines > 0 && IsFirstOnLine && _parent is BlockStatement && ((BlockStatement)_parent).IsCompactIfEmptyDefault)
                            EndNewLines = 0;
                    }

                    // Remove all blank lines at the end of a block
                    if (EndNewLines > 1)
                        EndNewLines = 1;
                }
                else
                {
                    // Add missing suggested braces
                    if (_codeObjects.Count > 0 && _parent is BlockStatement && ((BlockStatement)_parent).ShouldHaveBraces())
                        HasBraces = true;
                }
            }

        abort:
            // Restore the previous Unused list in the parser
            parser.PopUnusedList();

            // Post-processing: Associate comments with lone statements that follow them.
            // Scan bottom-up, so we can safely remove comments if we move them.
            for (int i = _codeObjects.Count - 2; i >= 0; --i)
            {
                // Look for comment objects that start on a new line
                CodeObject obj0 = _codeObjects[i];
                if (obj0 is CommentBase)
                    PostProcessComment((CommentBase)obj0, i);
            }

            if (!HasBraces)
            {
                // If we didn't have braces, set the block's IsFirstOnLine based on the object in it
                if (_codeObjects.Count > 0)
                {
                    IsFirstOnLine = _codeObjects[0].IsFirstOnLine;
                    EndNewLines = 0;
                }
                // If we had no children, no braces, and no empty statement, eliminate the Block object (can occur for SwitchItems)
                else if (_codeObjects.Count == 0 && !hasEmptyStatement)
                    parentBody = null;
            }
        }

        /// <summary>
        /// The number of code objects in the <see cref="Block"/>.
        /// </summary>
        public int Count
        {
            get { return _codeObjects.Count; }
        }

        /// <summary>
        /// The number of newlines preceeding the closing '}' (0 to N).
        /// </summary>
        public int EndNewLines
        {
            get { return (int)(_formatFlags & FormatFlags.NewLineMask); }
            set
            {
                SetNewLines(value);
                _formatFlags |= FormatFlags.NewLinesSet;
            }
        }

        /// <summary>
        /// True if the <see cref="Block"/> is delimited by braces.
        /// </summary>
        public bool HasBraces
        {
            get { return _formatFlags.HasFlag(FormatFlags.Grouping); }
            set
            {
                // Ignore any request to turn off braces if there are multiple objects in the body
                if (value || _codeObjects.Count <= 1)
                {
                    SetFormatFlag(FormatFlags.Grouping, value);
                    _formatFlags |= FormatFlags.GroupingSet;

                    if (value)
                    {
                        // If we're turning on braces, and first-on-line, force the end brace to be first-on-line
                        if (IsFirstOnLine && EndNewLines == 0)
                            EndNewLines = 1;
                    }
                    else
                    {
                        // If we're turning off braces, also clear EndNewLines
                        EndNewLines = 0;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the code object has a terminator character.
        /// </summary>
        public override bool HasTerminator
        {
            // Blocks don't have terminators, so disable their use of this flag
            get { return false; }
            set { }  // Just ignore any set attempts
        }

        /// <summary>
        /// The "Infix" End-Of-Line comment for the Initializer (if any) - appears after the open brace.
        /// </summary>
        /// <remarks>
        /// This property allows for the very convenient setting of Infix EOL comments in object initializers.
        /// Although there is support for multiple Infix EOL comments on the same object, this property doesn't
        /// support that, returning the first one that it finds, and replacing all existing ones when set.
        /// </remarks>
        public string InfixEOLComment
        {
            get
            {
                // Just return the first Infix EOL comment if there is more than one
                if (_annotations != null)
                {
                    Comment comment = (Comment)Enumerable.FirstOrDefault(_annotations, delegate (Annotation annotation) { return annotation is Comment && annotation.IsEOL && annotation.IsInfix; });
                    if (comment != null)
                        return comment.Text;
                }
                return null;
            }
            set
            {
                // Remove all existing Infix EOL comments before adding the new one
                RemoveAllAnnotationsWhere<Comment>(delegate (Comment annotation) { return annotation.IsEOL && annotation.IsInfix; });
                if (value != null)
                    AttachAnnotation(new Comment(value, CommentFlags.EOL) { IsInfix = true });
            }
        }

        /// <summary>
        /// Determines if the code object appears as the first item on a line.
        /// </summary>
        public override bool IsFirstOnLine
        {
            // Special flag for IsFirstOnLine for the '{', because the newlines storage is used for EndNewLines for the '}'
            get { return _formatFlags.HasFlag(FormatFlags.InfixNewLine); }
            set { SetFormatFlag(FormatFlags.InfixNewLine, value); }
        }

        /// <summary>
        /// True if the code object defaults to starting on a new line.
        /// </summary>
        public override bool IsFirstOnLineDefault
        {
            // Default to a single line (unless we have first-on-line annotations) - the block will be reformatted as items are added
            get { return HasFirstOnLineAnnotations; }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_codeObjects == null || _codeObjects.Count == 0 || (!_codeObjects[0].IsFirstOnLine && _codeObjects.IsSingleLine))); }
            set
            {
                // For Blocks, EndNewLines indicates the number of new lines before the '}'
                EndNewLines = (value ? 0 : 1);

                // Propagate the change to all members of the Block
                if (_codeObjects != null)
                {
                    CodeObject lastObj = null;
                    foreach (CodeObject obj in _codeObjects)
                    {
                        if (value)
                            obj.NewLines = 0;
                        else if (!obj.IsFirstOnLine)
                            obj.NewLines = (lastObj != null ? obj.DefaultNewLines(lastObj) : 1);
                        lastObj = obj;
                    }
                }
            }
        }

        /// <summary>
        /// True if the code object only requires a single line for display by default.
        /// </summary>
        public override bool IsSingleLineDefault
        {
            get
            {
                if (_codeObjects != null)
                {
                    int count = _codeObjects.Count;
                    if (count == 0)
                        return true;
                    if (count == 1)
                        return _codeObjects[0].IsSingleLineDefault;
                }
                return false;
            }
        }

        /// <summary>
        /// True if access to the <see cref="ICollection"/> is synchronized.
        /// </summary>
        public virtual bool IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Get the last <see cref="CodeObject"/> in the <see cref="Block"/>.
        /// </summary>
        public CodeObject Last
        {
            get { return _codeObjects.Last; }
        }

        /// <summary>
        /// The number of newlines preceeding the opening '{' (0 or 1 only - setting a higher value is ignored).
        /// </summary>
        public override int NewLines
        {
            get { return (IsFirstOnLine ? 1 : 0); }
            set { IsFirstOnLine = (value > 0); }
        }

        /// <summary>
        /// The parent <see cref="CodeObject"/>.
        /// </summary>
        public override CodeObject Parent
        {
            set
            {
                // If the parent is being set for the first time, force a reformat (we can't
                // do this in the constructor, because we don't have access to the parent yet).
                if (value is IBlock && (_parent == null || _codeObjects.Count <= 2))
                    ((IBlock)value).ReformatBlock();

                base.Parent = value;
                _codeObjects.Parent = value;
            }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ICollection"/>.
        /// </summary>
        public virtual object SyncRoot
        {
            get { return this; }
        }

        /// <summary>
        /// Get the child <see cref="CodeObject"/> at the specified index.
        /// </summary>
        public CodeObject this[int index]
        {
            get { return _codeObjects[index]; }
        }

        /// <summary>
        /// Parse any compiler directives between a statement header and body (or base type list, constructor initializer, or type constraints).
        /// Also used to parse any "open" conditional directives if 'includeAll' is false.
        /// </summary>
        public static bool ParseCompilerDirectives(Parser parser, CodeObject parent, AnnotationFlags position, bool includeAll)
        {
            // If the parent statement is followed by '#', then check if it's "sandwiched" by conditional directives
            bool processedPostAnnotations = false;
            if (parser.TokenText == CompilerDirective.ParseToken)
            {
                int openIfs = 0;
                bool hasUnusedIfFirst = false, hasUnusedElses = false;
                if (!includeAll)
                {
                    // If 'includeAll' is false, then we only want to parse any "open" conditional directives.
                    // Determine how many (if any) nested open conditional directives we have by looking both at existing
                    // annotations and also unused parser objects.  Stop if we hit a regular comment that isn't preceeded
                    // by a doc comment, or a global attribute.
                    if (parent.HasAnnotations)
                    {
                        for (int i = parent.Annotations.Count - 1; i >= 0; --i)
                        {
                            Annotation annotation = parent.Annotations[i];
                            if (annotation is EndIfDirective)
                                --openIfs;
                            else if (annotation is IfDirective)
                                ++openIfs;
                            else if ((annotation is Comment && (i == 0 || !(parent.Annotations[i - 1] is DocComment)))
                                || (annotation is Attribute && ((Attribute)annotation).IsGlobal))
                                break;
                        }
                    }
                    if (parser.HasUnused)
                    {
                        for (int i = parser.Unused.Count - 1; i >= 0; --i)
                        {
                            CodeObject unused = parser.GetUnusedCodeObject(i);
                            if (unused is EndIfDirective)
                                --openIfs;
                            else if (unused is IfDirective)
                            {
                                ++openIfs;
                                if (i == 0)
                                    hasUnusedIfFirst = true;
                            }
                            else if (unused is ConditionalDirective)
                                hasUnusedElses = true;
                            else if ((unused is Comment && (i == 0 || !(parser.GetUnusedCodeObject(i - 1) is DocComment)))
                                || (unused is Attribute && ((Attribute)unused).IsGlobal))
                                break;
                        }
                    }
                    if (openIfs <= 0)
                        return false;
                }

                // Move any compiler directives to the parent as postfix directives
                int endifCount = 0;
                CodeObject firstConditional = null;
                do
                {
                    // Special case - abort if not including all and statement is just wrapped in #if/#endif
                    if (!includeAll && hasUnusedIfFirst && !hasUnusedElses && parser.PeekNextTokenText() == EndIfDirective.ParseToken && !processedPostAnnotations)
                        return false;

                    CodeObject obj = parser.ProcessToken(parent);
                    if (obj is CompilerDirective)
                    {
                        // Move trailing directives to the parent (whether it has immediately preceeding directives or not),
                        // counting total '#endif's.
                        if (obj is ConditionalDirectiveBase)
                        {
                            if (firstConditional == null)
                                firstConditional = obj;
                            if (obj is EndIfDirective)
                                ++endifCount;
                        }
                        parent.AttachAnnotation((Annotation)obj, position);
                        parent.MoveCommentsAsPost(parser.LastToken);
                        processedPostAnnotations = true;
                    }
                }
                while (parser.TokenText == CompilerDirective.ParseToken && (includeAll || endifCount < openIfs));

                // If the first trailing conditional directive wasn't an '#if', check for preceeding directives
                if (!(firstConditional is IfDirective))
                {
                    // If the parent statement is preceeded by an '#if', '#elif', or '#else', move all preceeding
                    // conditional directives to the parent (it's "sandwiched" by them), up to the total 'endifCount'
                    // (stop after that to avoid attaching compiler directives that are unrelated to the statement).
                    if (parser.LastUnusedCodeObject is ConditionalDirective)
                        parent.ParseUnusedAnnotations(parser, parent, true, (endifCount < 1 ? 1 : endifCount));
                }
            }
            return processedPostAnnotations;
        }

        /// <summary>
        /// Parse any compiler directives between a statement header and body (or base type list, constructor initializer, or type constraints).
        /// </summary>
        public static bool ParseCompilerDirectives(Parser parser, CodeObject parent, AnnotationFlags position)
        {
            return ParseCompilerDirectives(parser, parent, position, true);
        }

        /// <summary>
        /// Skip parsing a brace-delimited <see cref="Block"/>.
        /// </summary>
        public static void SkipParsingBlock(Parser parser, CodeObject parent, bool bracesRequired, params string[] terminators)
        {
            // Don't process comments or conditional directives if we have no parent, or the parent has no header (CodeUnit or BlockDecl)
            if (parent is IBlock && ((IBlock)parent).HasHeader)
            {
                // Process comments
                parent.MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment with the parent

                // Toss any skipped comment objects
                parser.LastToken.TrailingComments = null;

                // Parse any post compiler directives between the parent statement and the block
                ParseCompilerDirectives(parser, parent, AnnotationFlags.IsPostfix);
            }
            else
            {
                // Toss any skipped comments at the top of a CodeUnit or BlockDecl (from the dummy token)
                parser.LastToken.TrailingComments = null;
            }

            // Parse the braces and everything between them, throwing it all away
            if (parser.TokenText == ParseTokenStart)
            {
                parser.NextToken();                        // Move past '{'
                parser.LastToken.TrailingComments = null;  // Toss any comments
                int nestLevel = 0;
                while (parser.Token != null && !(parser.TokenText == ParseTokenEnd && nestLevel == 0))
                {
                    if (parser.TokenText == ParseTokenStart)
                        ++nestLevel;
                    else if (parser.TokenText == ParseTokenEnd)
                        --nestLevel;
                    if (parser.TokenType == TokenType.CompilerDirective)
                        parser.ProcessToken(parent);  // Skip & toss any compiler directives
                    else
                        parser.NextToken();
                    parser.LastToken.TrailingComments = null;  // Toss any comments
                }
                if (parser.TokenText == ParseTokenEnd)
                    parser.NextToken();  // Move past '}'
            }
        }

        /// <summary>
        /// Add a code object to the <see cref="Block"/>.
        /// </summary>
        /// <param name="codeObject">The object to be added.</param>
        public void Add(CodeObject codeObject)
        {
            AddInternal(codeObject);
            ObjectCountChanged();
        }

        /// <summary>
        /// Add multiple code objects to the <see cref="Block"/>.
        /// </summary>
        /// <param name="codeObjects">The objects to be added.</param>
        public void Add(params CodeObject[] codeObjects)
        {
            foreach (CodeObject codeObject in codeObjects)
                Add(codeObject);
        }

        /// <summary>
        /// Add a collection of code objects to the <see cref="Block"/>.
        /// </summary>
        /// <param name="collection">The collection to be added.</param>
        public void AddRange(IEnumerable<CodeObject> collection)
        {
            foreach (CodeObject codeObject in collection)
                Add(codeObject);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            if (IsGenerated)
                return;
            if (flags.HasFlag(RenderFlags.Description))
            {
                TypeRefBase.AsTextType(writer, GetType(), RenderFlags.None);
                return;
            }

            bool isTopLevel = (!(_parent is IBlock) || ((IBlock)_parent).IsTopLevel);
            if (isTopLevel)
                flags |= RenderFlags.NoBlockIndent;
            bool updatedLineCol = false;

            // Render the open brace if appropriate
            bool useBraces = HasBraces && !isTopLevel;
            if (useBraces || HasFirstOnLineAnnotations)
            {
                if (IsFirstOnLine)
                {
                    if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                        writer.WriteLine();
                }
                else
                    writer.Write(" ");

                AsTextBefore(writer, flags);
                if (useBraces)
                {
                    UpdateLineCol(writer, flags);
                    updatedLineCol = true;
                    writer.Write(ParseTokenStart);
                }
                flags &= ~RenderFlags.SuppressNewLine;
            }

            if (!flags.HasFlag(RenderFlags.NoEOLComments))
                AsTextInfixEOLComments(writer, flags);

            // Increase the indent level for any newlines that occur within the block
            bool increaseIndent = !flags.HasFlag(RenderFlags.NoBlockIndent);
            if (increaseIndent)
                writer.BeginIndentOnNewLine(this);

            // Render the body of the block
            bool isSingleLineBody = true;
            int codeObjectCount = (_codeObjects != null ? _codeObjects.Count : 0);

            // Render an empty statement ';' if we have no children or a single comment and no braces
            if ((codeObjectCount == 0 || (codeObjectCount == 1 && _codeObjects[0] is Comment)) && !HasBraces && (!(_parent is BlockStatement) || ((BlockStatement)_parent).RequiresEmptyStatement))
            {
                if (IsFirstOnLine)
                {
                    if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                        writer.WriteLine();
                }
                else
                    writer.Write(" ");
                writer.Write(Statement.ParseTokenTerminator);
                if (codeObjectCount > 0)
                    writer.Write("  ");
                flags |= RenderFlags.SuppressNewLine;
            }

            int endAlignmentAt = -1;
            RenderFlags passFlags = (flags & (RenderFlags.PassMask | RenderFlags.SuppressNewLine)) | RenderFlags.PrefixSpace;
            for (int i = 0; i < codeObjectCount; ++i)
            {
                CodeObject codeObject = _codeObjects[i];
                if (codeObject.IsGenerated)
                    continue;

                // Check for newlines
                if (codeObject.NewLines > 0)
                    isSingleLineBody = false;

                // Check for possible alignment of initializations and/or EOL comments on consecutive code objects
                if (i > endAlignmentAt && !isSingleLineBody && codeObject.HasEOLComments && codeObject.IsSingleLine)
                {
                    int j = i;
                    int maxCodeWidth = 0;
                    int maxCommentWidth = 0;
                    do
                    {
                        CodeObject current = _codeObjects[j];

                        // Ignore hidden generated objects or comments without preceeding blank lines
                        if (!current.IsGenerated && !(current is CommentBase && current.NewLines == 1))
                        {
                            // Abort if we hit a blank line, or an object that doesn't have an EOL comment or isn't single-line
                            if (j > i && (current.NewLines > 1 || !current.HasEOLComments || !current.IsSingleLine))
                                break;

                            // Determine the padding width needed for alignment
                            int length = current.AsTextLength();
                            int newMaxCodeWidth = (length > maxCodeWidth ? length : maxCodeWidth);
                            int commentLength = current.EOLComment.Length;
                            int newMaxCommentWidth = (commentLength > maxCommentWidth ? commentLength : maxCommentWidth);

                            // Abort if the padded line length would be too long (IF the current line has some padding)
                            if ((writer.IndentOffset + newMaxCodeWidth + 5 + newMaxCommentWidth > MaximumLineLength)
                                && (length < newMaxCodeWidth || commentLength < newMaxCommentWidth))
                                break;

                            maxCodeWidth = newMaxCodeWidth;
                            maxCommentWidth = newMaxCommentWidth;
                        }
                        ++j;
                    }
                    while (j < _codeObjects.Count);

                    if (--j > i)
                    {
                        writer.BeginAlignment(this, new[] { maxCodeWidth });
                        endAlignmentAt = j;
                    }
                }

                // If the block's line/col info hasn't been set (no braces), we want it to match the first child, but this requires
                // processing any pending newline first, and then telling the child to suppress it.  We can't just render the child
                // first and use its line/col info, because an Assignment's line/col is the '=' character's position, and there might
                // also be prefixed annotations.
                if (i == 0 && !updatedLineCol && flags.HasFlag(RenderFlags.UpdateLineCol))
                {
                    int newLines = NewLines;
                    bool isPrefix = passFlags.HasFlag(RenderFlags.IsPrefix);
                    if (!isPrefix && newLines > 0)
                    {
                        if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                        {
                            writer.WriteLines(codeObject.NewLines);
                            passFlags |= RenderFlags.SuppressNewLine;
                        }
                    }
                    else
                    {
                        writer.Write(" ");
                        passFlags &= ~RenderFlags.PrefixSpace;
                    }

                    UpdateLineCol(writer, flags);
                    updatedLineCol = true;
                }

                // Render the code object
                codeObject.AsText(writer, passFlags | (isSingleLineBody ? 0 : RenderFlags.IncreaseIndent));
                passFlags = (passFlags & ~RenderFlags.SuppressNewLine) | RenderFlags.PrefixSpace;
                flags &= ~RenderFlags.SuppressNewLine;

                // End any pending alignment if it's time
                if (i == endAlignmentAt)
                    writer.EndAlignment(this);
            }

            // Revert the indent level
            if (increaseIndent)
                writer.EndIndentation(this);

            // Render the close brace if appropriate
            if (useBraces)
            {
                int endNewLines = EndNewLines;
                if (endNewLines > 0)
                    writer.WriteLines(endNewLines);
                else
                    writer.Write(" ");
                writer.Write(ParseTokenEnd);
            }

            // Render any EOL comments (rendered after the close brace when it's on a line by itself - special Infix
            // EOL comments may also exist and be rendered after the open brace above).
            if (!flags.HasFlag(RenderFlags.NoEOLComments))
                AsTextEOLComments(writer, RenderFlags.None);

            AsTextAfter(writer, flags);
        }

        /// <summary>
        /// Clear all members from the <see cref="Block"/>.
        /// </summary>
        public void Clear()
        {
            RemoveAll();
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Block clone = (Block)base.Clone();
            clone._codeObjects = ChildListHelpers.Clone(_codeObjects, null);
            // Re-build the dictionary using the cloned objects
            clone.RebuildDictionary();
            return clone;
        }

        /// <summary>
        /// Check if the <see cref="Block"/> contains the specified <see cref="CodeObject"/>.
        /// </summary>
        /// <param name="codeObject">The object being searched for.</param>
        /// <returns>True if the block contains the object, otherwise false.</returns>
        public bool Contains(CodeObject codeObject)
        {
            return Enumerable.Any(_codeObjects, delegate (CodeObject child) { return child == codeObject; });
        }

        /// <summary>
        /// Copy the code objects in the block to the specified array, starting at the specified offset.
        /// </summary>
        /// <param name="codeObjects">The array to copy into.</param>
        /// <param name="index">The starting index in the array.</param>
        public void CopyTo(CodeObject[] codeObjects, int index)
        {
            CopyTo((Array)codeObjects, index);
        }

        /// <summary>
        /// Copy the code objects in the block to the specified array, starting at the specified offset.
        /// </summary>
        /// <param name="array">The array to copy into.</param>
        /// <param name="index">The starting index in the array.</param>
        public void CopyTo(Array array, int index)
        {
            if (array == null)
                throw new ArgumentNullException("array", "Null array reference");
            if (index < 0)
                throw new ArgumentOutOfRangeException("index", "Index is out of range");
            if (array.Rank > 1)
                throw new ArgumentException("Array is multi-dimensional", "array");

            foreach (CodeObject obj in _codeObjects)
                array.SetValue(obj, index++);
        }

        /// <summary>
        /// Enumerate all children with the specified name.
        /// </summary>
        public IEnumerable<CodeObject> Find(string name)
        {
            INamedCodeObject foundObj = FindChildren(name);
            if (foundObj is NamedCodeObjectGroup)
            {
                foreach (INamedCodeObject namedCodeObject in (NamedCodeObjectGroup)foundObj)
                    yield return (CodeObject)namedCodeObject;
            }
            else
                yield return (CodeObject)foundObj;
        }

        /// <summary>
        /// Enumerate all children with the specified name and type.
        /// </summary>
        public IEnumerable<T> Find<T>(string name) where T : CodeObject
        {
            INamedCodeObject foundObj = FindChildren(name);
            if (foundObj is NamedCodeObjectGroup)
            {
                foreach (INamedCodeObject namedCodeObject in (NamedCodeObjectGroup)foundObj)
                {
                    if (namedCodeObject is T)
                        yield return (T)namedCodeObject;
                }
            }
            else if (foundObj is T)
                yield return (T)foundObj;
        }

        /// <summary>
        /// Enumerate all children of type T.
        /// </summary>
        public IEnumerable<T> Find<T>() where T : CodeObject
        {
            return Enumerable.OfType<T>(_codeObjects);
        }

        /// <summary>
        /// Find children with the specified name.
        /// </summary>
        /// <returns>A <see cref="CodeObject"/>, <see cref="NamedCodeObjectGroup"/>, or null if no matches were found.</returns>
        public INamedCodeObject FindChildren(string name)
        {
            return (_namedMembers != null ? _namedMembers.Find(name) : null);
        }

        /// <summary>
        /// Find children with the specified name having type T, adding them to the specified results collection.
        /// </summary>
        public void FindChildren<T>(string name, NamedCodeObjectGroup results) where T : CodeObject
        {
            INamedCodeObject foundObj = FindChildren(name);
            if (foundObj is NamedCodeObjectGroup)
            {
                foreach (INamedCodeObject namedCodeObject in (NamedCodeObjectGroup)foundObj)
                {
                    if (namedCodeObject is T)
                        results.Add(namedCodeObject);
                }
            }
            else if (foundObj is T)
                results.Add(foundObj);
        }

        /// <summary>
        /// Find the first child object with the specified name and type.
        /// </summary>
        public T FindFirst<T>(string name) where T : CodeObject
        {
            return Enumerable.FirstOrDefault(Find<T>(name));
        }

        /// <summary>
        /// Find the first child object of type T.
        /// </summary>
        public T FindFirst<T>() where T : CodeObject
        {
            return Enumerable.FirstOrDefault(Enumerable.OfType<T>(_codeObjects));
        }

        /// <summary>
        /// Find the index of the specified <see cref="CodeObject"/> in the <see cref="Block"/>.
        /// </summary>
        /// <param name="codeObject">The object being searched for.</param>
        /// <returns>The index of the code object, or -1 if not found.</returns>
        public int FindIndexOf(CodeObject codeObject)
        {
            for (int i = 0; i < _codeObjects.Count; ++i)
                if (_codeObjects[i] == codeObject)
                    return i;
            return -1;
        }

        public IEnumerable<T> GetChildren<T>()
            where T : CodeObject
        {
            var res = new List<T>();
            foreach (var c in this)
            {
                if (c is T cc)
                {
                    res.Add(cc);
                }
                else if (c is Block blk)
                {
                    res.AddRange(blk.GetChildren<T>());
                }
            }

            return res;
        }

        /// <summary>
        /// Get an enumerator for the code objects in the <see cref="Block"/>.
        /// </summary>
        IEnumerator<CodeObject> IEnumerable<CodeObject>.GetEnumerator()
        {
            return ((IEnumerable<CodeObject>)_codeObjects).GetEnumerator();
        }

        /// <summary>
        /// Get an enumerator for the code objects in the <see cref="Block"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _codeObjects.GetEnumerator();
        }

        /// <summary>
        /// Insert a code object into the block at the specified index.
        /// </summary>
        /// <param name="index">The index at which to insert.</param>
        /// <param name="codeObject">The object to be inserted.</param>
        public void Insert(int index, CodeObject codeObject)
        {
            if (codeObject is Block)
            {
                ChildList<CodeObject> codeObjects = ((Block)codeObject)._codeObjects;
                for (int i = 0; i < codeObjects.Count; ++i)
                    Insert(i, codeObjects[i]);
            }
            else
            {
                AddInsertFormattingCheck(index, codeObject);
                InsertInternalNoFormatting(index, codeObject);
                ObjectCountChanged();
            }
        }

        /// <summary>
        /// Parse an Expression into the Block.
        /// </summary>
        public void ParseExpressionAsBlock(Parser parser, CodeObject parent)
        {
            HasBraces = false;
            AddInternalNoFormatting(Expression.Parse(parser, parent, true));
        }

        /// <summary>
        /// Re-build the internal dictionary of named code objects in the <see cref="Block"/>.
        /// </summary>
        public void RebuildDictionary()
        {
            _namedMembers = new NamedCodeObjectDictionary();
            if (_codeObjects != null)
            {
                foreach (CodeObject codeObject in _codeObjects)
                {
                    if (codeObject is INamedCodeObject)
                        ((INamedCodeObject)codeObject).AddToDictionary(_namedMembers);
                }
            }
        }

        /// <summary>
        /// Remove the specified <see cref="CodeObject"/> from the <see cref="Block"/>.
        /// </summary>
        /// <returns>True if the code object was found and removed, otherwise false.</returns>
        public bool Remove(CodeObject codeObject)
        {
            bool removed = _codeObjects.Remove(codeObject);
            RemoveInternal(codeObject);
            return removed;
        }

        /// <summary>
        /// Remove all code objects from the <see cref="Block"/>.
        /// </summary>
        public void RemoveAll()
        {
            foreach (CodeObject codeObject in _codeObjects)
            {
                if (codeObject is Annotation && ((Annotation)codeObject).IsListed)
                    NotifyListedAnnotationRemoved((Annotation)codeObject);
            }
            _codeObjects.Clear();
            _namedMembers.Clear();
            ObjectCountChanged();
        }

        /// <summary>
        /// Remove the <see cref="CodeObject"/> at the specified index from the <see cref="Block"/>.
        /// </summary>
        public void RemoveAt(int index)
        {
            CodeObject codeObject = this[index];
            _codeObjects.RemoveAt(index);
            RemoveInternal(codeObject);
        }

        /// <summary>
        /// Replace the specified <see cref="CodeObject"/> with a new one.
        /// </summary>
        /// <returns>True if the code object was found and replaced, otherwise false.</returns>
        public bool Replace(CodeObject oldObject, CodeObject newObject)
        {
            int index = FindIndexOf(oldObject);
            if (index >= 0)
            {
                RemoveAt(index);
                Insert(index, newObject);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Flush unused objects in the parser into the Block.
        /// </summary>
        protected internal void FlushUnused(Parser parser)
        {
            if (parser.HasUnused)
            {
                // Special case: If the code is embedded in a '<code>' doc comment tag (we got here from Parser.ParseCodeBlockUntil),
                // and we have an empty block so far, and only a single unused object, just flush it directly to the block.
                if (parser.InDocComment && _codeObjects.Count == 0 && parser.Unused.Count == 1)
                {
                    ParsedObject unused = parser.RemoveLastUnused();
                    CodeObject obj;
                    if (unused is Token)
                        obj = new UnresolvedRef((Token)unused);
                    else
                        obj = ((UnusedCodeObject)unused).CodeObject;
                    _codeObjects.Add(obj);
                }
                else
                {
                    // Flush unused objects
                    Unrecognized unrecognized = null;
                    foreach (ParsedObject unused in parser.Unused)
                    {
                        // Flush unused tokens and expressions as Unrecognized object expressions
                        if (unused is Token || (unused is UnusedCodeObject && ((UnusedCodeObject)unused).CodeObject is Expression))
                        {
                            // Get the unused object as an expression
                            Expression expression = (unused is Token ? new UnresolvedRef((Token)unused) : (Expression)((UnusedCodeObject)unused).CodeObject);

                            // Flush the Unrecognized object anytime we start a new line
                            if (expression.IsFirstOnLine)
                                FlushUnrecognized(parser, ref unrecognized);

                            // Add the expression to the Unrecognized object
                            if (unrecognized == null)
                                unrecognized = new Unrecognized(true, unused.InDocComment, expression);
                            else
                                unrecognized.AddRight(expression);

                            // If the unused object has trailing comments, flush them now
                            if (unused.HasTrailingComments)
                            {
                                unrecognized.MoveEOLComment(unused.AsToken());  // Associate any skipped EOL comment
                                FlushUnrecognized(parser, ref unrecognized);    // Flush the Unrecognized object
                                AddTrailingComments(unused.AsToken());          // Add skipped Comment objects
                            }
                        }
                        else
                        {
                            // Flush any Unrecognized object
                            FlushUnrecognized(parser, ref unrecognized);

                            // Flush the non-Expression code object (such as a CompilerDirective or Comment)
                            UnusedCodeObject unusedCodeObject = (UnusedCodeObject)unused;
                            CodeObject codeObject = unusedCodeObject.CodeObject;
                            AddInternalNoFormatting(codeObject);
                            if (codeObject is CommentBase)
                                AdjustCommentIndentation((CommentBase)codeObject);
                            AddTrailingComments(unusedCodeObject.LastToken);  // Add skipped Comment objects
                        }
                    }

                    // Final flush of the Unrecognized object
                    FlushUnrecognized(parser, ref unrecognized);

                    parser.Unused.Clear();
                }
            }
        }

        /// <summary>
        /// Post-process the specified comment object at the specified index, associating with the following object if applicable.
        /// </summary>
        protected internal void PostProcessComment(CommentBase obj, int index)
        {
            if (obj.IsFirstOnLine && index < _codeObjects.Count - 1)
            {
                // Check for following statements or expressions without a preceeding blank line, or allow a couple
                // of blank lines if the comment is a documentation comment.
                CodeObject obj1 = _codeObjects[index + 1];
                int obj1NewLines = obj1.NewLines;
                if (obj1.AssociateCommentWhenParsing(obj) && (obj1NewLines <= 1 || (obj is DocComment && obj1NewLines <= 3)))
                {
                    // Remove any blank lines between DocComments and following code
                    if (obj1NewLines > 1)
                        obj1.NewLines = 1;

                    // Check that the following statement is also on a new line
                    if (obj1.IsFirstOnLine)
                    {
                        // Check if the candidate is the last object in the block, or is followed by a blank line, or is followed by
                        // either a Comment or an object with first-on-line annotations, or is followed by a compiler directive, or if
                        // the candidate comment is a documentation comment - in these cases, associate the candidate comment with the
                        // object that follows it.
                        CodeObject obj2 = (index < _codeObjects.Count - 2 ? _codeObjects[index + 2] : null);
                        if (obj2 == null || obj2.NewLines > 1 || obj2 is CommentBase || obj2.HasFirstOnLineAnnotations || obj2 is CompilerDirective || obj is DocComment)
                        {
                            // Special exception: Also require that either the comment is preceeded by a blank line, or less than two
                            // code objects, or the preceeding object is a comment or has a blank line before it, or the 2nd preceeding
                            // object is a comment.
                            if (obj.NewLines > 1 || index < 2 || _codeObjects[index - 1] is CommentBase || _codeObjects[index - 1].NewLines > 1 || _codeObjects[index - 2] is CommentBase)
                            {
                                RemoveAt(index);
                                obj1.AttachAnnotation(obj, true);
                            }
                        }
                    }
                    else
                    {
                        // If the following statement is inline, associate the inline comment with it
                        RemoveAt(index);
                        obj1.AttachAnnotation(obj, true);
                        obj1.IsFirstOnLine = true;
                        obj.IsFirstOnLine = false;
                    }
                }
            }
        }

        protected void AddInsertFormattingCheck(int index, CodeObject codeObject)
        {
            // Default the # of newlines for the object if it wasn't already explicitly specified
            if (!codeObject.IsNewLinesSet)
            {
                int newLines;

                // If we already have items in the block, determine newlines based upon the previous item
                CodeObject previous = null;
                for (int i = index - 1; i >= 0 && i < _codeObjects.Count; --i)
                {
                    if (!_codeObjects[i].IsGenerated)
                    {
                        previous = _codeObjects[i];
                        break;
                    }
                }
                if (previous != null)
                    newLines = codeObject.DefaultNewLines(previous);
                else
                {
                    // If we're adding or inserting the first item, then if either brace has a newline, use one, otherwise none
                    newLines = ((IsFirstOnLine || EndNewLines > 0) ? 1 : 0);
                    // If the added object has a newline, and the closing brace doesn't, make it have one now
                    if (newLines > 0 && EndNewLines == 0)
                        EndNewLines = 1;
                }
                codeObject.SetNewLines(newLines);
            }

            // Check for stand-alone expressions
            if (codeObject is Expression)
            {
                // Turn on the terminator, and default parens to off
                codeObject.HasTerminator = true;
                if (!codeObject.IsGroupingSet && ((Expression)codeObject).HasParens)
                    codeObject.SetFormatFlag(FormatFlags.Grouping, false);
            }
        }

        protected void AddInternal(CodeObject codeObject)
        {
            if (codeObject is Block)
            {
                foreach (CodeObject obj in ((Block)codeObject)._codeObjects)
                    AddInternal(obj);
            }
            else
            {
                AddInsertFormattingCheck(_codeObjects.Count, codeObject);
                AddInternalNoFormatting(codeObject);
            }
        }

        protected void AddInternalNoFormatting(CodeObject codeObject)
        {
            // Add the code object to the block
            _codeObjects.Add(codeObject);

            // Add named members to the block's dictionary
            if (codeObject is INamedCodeObject)
                AddNamedMember((INamedCodeObject)codeObject);

            // If any annotations are added directly to the block, send notifications
            // (this will send special comments up to the CodeUnit and Solution levels).
            if (codeObject is Annotation && ((Annotation)codeObject).IsListed)
                NotifyListedAnnotationAdded((Annotation)codeObject);
        }

        protected void AddNamedMember(INamedCodeObject namedCodeObject)
        {
            if (_namedMembers == null)
                _namedMembers = new NamedCodeObjectDictionary();
            namedCodeObject.AddToDictionary(_namedMembers);

            // If a TypeDecl is added to a NamespaceDecl, also add it to the Namespace
            if (_parent is NamespaceDecl && namedCodeObject is TypeDecl)
            {
                Namespace @namespace = ((NamespaceDecl)_parent).Namespace;
                if (@namespace != null)
                    @namespace.Add((TypeDecl)namedCodeObject);
            }
        }

        /// <summary>
        /// Add any trailing Comment objects on the specified token to the Block.
        /// </summary>
        protected void AddTrailingComments(Token token)
        {
            if (token != null && token.TrailingComments != null)
            {
                foreach (CommentBase commentBase in token.TrailingComments)
                {
                    // Add the comment first (so it acquires a parent), then adjust it
                    AddInternalNoFormatting(commentBase);
                    AdjustCommentIndentation(commentBase);
                }
                token.TrailingComments = null;

                // If the block has no braces, force the Line/Col info to match the first comment (if any)
                if (!HasBraces && Count > 0)
                    SetLineCol(this[0]);
            }
        }

        // Flush the Unrecognized object and clear it
        protected void FlushUnrecognized(Parser parser, ref Unrecognized unrecognized)
        {
            if (unrecognized != null)
            {
                unrecognized.HasTerminator = false;  // Force the terminator off
                unrecognized.UpdateMessage();
                _codeObjects.Add(unrecognized);
                unrecognized = null;
            }
        }

        protected void InsertInternalNoFormatting(int index, CodeObject codeObject)
        {
            // Insert the code object into the block
            _codeObjects.Insert(index, codeObject);

            // Add named members to the block's dictionary
            if (codeObject is INamedCodeObject)
                AddNamedMember((INamedCodeObject)codeObject);

            // If any annotations are inserted directly to the block, send notifications
            // (this will send special comments up to the CodeUnit and Solution levels).
            if (codeObject is Annotation && ((Annotation)codeObject).IsListed)
                NotifyListedAnnotationAdded((Annotation)codeObject);
        }

        protected void ObjectCountChanged()
        {
            if (_parent is IBlock && _codeObjects.Count <= 2)
                ((IBlock)_parent).ReformatBlock();
        }

        protected void RemoveInternal(CodeObject codeObject)
        {
            if (codeObject is INamedCodeObject)
                ((INamedCodeObject)codeObject).RemoveFromDictionary(_namedMembers);
            if (codeObject is Annotation && ((Annotation)codeObject).IsListed)
                NotifyListedAnnotationRemoved((Annotation)codeObject);
            ObjectCountChanged();
        }
    }
}