using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Base
{
    /// <summary>
    /// The common base class of all statements (<see cref="BlockStatement"/>, <see cref="VariableDecl"/>, <see cref="UsingDirective"/>,
    /// <see cref="Break"/>, <see cref="Continue"/>, <see cref="Goto"/>, <see cref="Label"/>, <see cref="Return"/>, <see cref="Throw"/>,
    /// <see cref="YieldStatement"/>, <see cref="DoWhile"/>, <see cref="Alias"/>, <see cref="ExternAlias"/>).
    /// </summary>
    /// <remarks>
    /// Common features provided by this class include handling of indentation (or optional inlining) and
    /// association of both normal comments and EOL comments.  Additional features include an optional prefix
    /// to the statement, optional statement keyword, an optional argument with optional parentheses, and
    /// an optional terminator.
    /// </remarks>
    public abstract class Statement : CodeObject
    {
        /// <summary>
        /// The token used to parse the terminator for a <see cref="Statement"/>.
        /// </summary>
        public const string ParseTokenTerminator = ";";

        protected Statement()
        {
            HasTerminator = HasTerminatorDefault;
        }

        /// <summary>
        /// Create a code object from an existing one, copying members.
        /// </summary>
        protected Statement(Statement statement)
            : base(statement)
        { }

        protected Statement(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public virtual bool HasArgument
        {
            get { return true; }  // Default is argument exists
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public virtual bool HasArgumentParens
        {
            get { return true; }  // Default is argument has parens
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public virtual bool HasTerminatorDefault
        {
            get { return false; }  // Default is no terminator
        }

        /// <summary>
        /// True if the closing paren or bracket is on a new line.
        /// </summary>
        public bool IsEndFirstOnLine
        {
            get { return _formatFlags.HasFlag(FormatFlags.InfixNewLine); }
            set { SetFormatFlag(FormatFlags.InfixNewLine, value); }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/> (null if none).
        /// </summary>
        public virtual string Keyword
        {
            get { return null; }
        }

        /// <summary>
        /// The number of newlines preceeding the object (0 to N).
        /// </summary>
        public override int NewLines
        {
            get { return base.NewLines; }
            set
            {
                // If we're changing to non-zero, also change all prefix comments to non-zero
                if (_annotations != null && value != 0 && !IsFirstOnLine)
                    SetFirstOnLineForNonEOLComments(_annotations, true);
                base.NewLines = value;
            }
        }

        /// <summary>
        /// The terminator character for the <see cref="Statement"/>.
        /// </summary>
        public virtual string Terminator
        {
            get { return ParseTokenTerminator; }  // Default terminator character
        }

        public override T Accept<T>(VisitorBase<T> visitor)
        {
            return visitor.Visit(this);
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            // Only associate regular comments with statements by default, not doc comments
            // (this will be overridden for TypeDecls and type member decls).
            return (comment is Comment);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            int newLines = NewLines;
            bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
            if (!isPrefix && newLines > 0)
            {
                if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
            }
            else if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");
            flags &= ~RenderFlags.SuppressNewLine;

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);

            // Increase the indent level for any newlines that occur within the statement if the flag is set
            bool increaseIndent = flags.HasFlag(RenderFlags.IncreaseIndent);
            if (increaseIndent)
                writer.BeginIndentOnNewLine(this);

            AsTextPrefix(writer, flags);
            AsTextStatement(writer, flags);
            if (HasArgument)
            {
                bool hasParens = HasArgumentParens;
                AsTextArgumentPrefix(writer, passFlags);
                if (hasParens)
                    writer.Write(Expression.ParseTokenStartGroup);
                AsTextArgument(writer, passFlags);
                if (hasParens)
                {
                    if (IsEndFirstOnLine)
                        writer.WriteLine();
                    writer.Write(Expression.ParseTokenEndGroup);
                }
            }
            AsTextSuffix(writer, flags);
            AsTextEOLComments(writer, flags);

            if (increaseIndent)
                writer.EndIndentation(this);

            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));

            if (isPrefix)
            {
                // If this object is rendered as a child prefix object of another, then IsFirstOnLine actually
                // represents IsLastOnLine, and we render the whitespace here at the end instead of at the top.
                if (newLines > 0)
                    writer.WriteLines(newLines);
                else
                    writer.Write(" ");
            }
        }

        protected internal void AsTextTerminator(CodeWriter writer, RenderFlags flags)
        {
            if (HasTerminator)
            {
                writer.Write(Terminator);
                CheckForAlignment(writer);  // Check for alignment of any EOL comments
            }
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.NoPostAnnotations))
                AsTextAnnotations(writer, AnnotationFlags.IsPostfix, flags);
        }

        protected virtual void AsTextArgument(CodeWriter writer, RenderFlags flags)
        { }

        protected virtual void AsTextArgumentPrefix(CodeWriter writer, RenderFlags flags)
        {
            writer.Write(" ");
        }

        protected virtual void AsTextPrefix(CodeWriter writer, RenderFlags flags)
        { }

        protected virtual void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            writer.Write(Keyword);
        }

        protected virtual void AsTextSuffix(CodeWriter writer, RenderFlags flags)
        {
            if (!flags.HasFlag(RenderFlags.Description))
                AsTextTerminator(writer, flags);
        }

        protected override void DefaultFormatField(CodeObject field)
        {
            base.DefaultFormatField(field);

            // Turn off parens for child expressions by default
            if (field is Expression && (((Expression)field).HasParens && !field.IsGroupingSet))
                field.SetFormatFlag(FormatFlags.Grouping, false);
        }

        protected void ParseKeywordAndArgument(Parser parser, ref Expression argument)
        {
            parser.NextToken();  // Move past the keyword

            Token firstToken = parser.Token;
            bool openParen = ParseExpectedToken(parser, Expression.ParseTokenStartGroup);  // Move past '('

            SetField(ref argument, Expression.Parse(parser, this, true, Expression.ParseTokenEndGroup), false);
            if (openParen)
            {
                // Move any comments after the '(' to the argument expression
                if (argument != null)
                    argument.MoveCommentsToLeftMost(firstToken, false);

                if (ParseExpectedToken(parser, Expression.ParseTokenEndGroup))  // Move past ')'
                {
                    if (parser.LastToken.IsFirstOnLine)
                        IsEndFirstOnLine = true;
                }
            }
        }

        /// <summary>
        /// Parse the terminator at the end of a <see cref="Statement"/>.
        /// </summary>
        protected void ParseTerminator(Parser parser)
        {
            MoveCommentsAsPost(parser.LastToken);   // Get any comments before the ';' as post comments

            // Parse the terminator, attaching an error if it's missing
            if (parser.TokenText == Terminator)
            {
                HasTerminator = true;
                parser.NextToken();
            }
            else
                parser.AttachMessage(this, "'" + Terminator + "' expected", parser.Token);
        }

        /// <summary>
        /// This method is used when we must parse backwards through the Unused list to get a type.
        /// </summary>
        protected void ParseUnusedType(Parser parser, ref Expression type)
        {
            if (!ModifiersHelpers.IsModifier(parser.LastUnusedTokenText))
            {
                Expression expression = parser.RemoveLastUnusedExpression();
                MoveFormatting(expression);
                SetField(ref type, expression, false);
            }
        }
    }
}