using System;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all compiler directives (<see cref="ConditionalDirective"/>, <see cref="MessageDirective"/>,
    /// <see cref="SymbolDirective"/>, <see cref="PragmaDirective"/>, <see cref="LineDirective"/>).
    /// </summary>
    /// <remarks>
    /// Compiler directives are always prefixed with a '#', which must be the first token on the line, although it may
    /// have whitespace before it and/or between it and the directive name.
    /// </remarks>
    public abstract class CompilerDirective : Annotation
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "#";

        protected CompilerDirective()
        {
            if (HasNoIndentationDefault)
                SetFormatFlag(FormatFlags.NoIndentation, true);
        }

        protected CompilerDirective(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            if (!parser.Token.IsFirstOnLine)
                parser.AttachMessage(this, "'#' must be the first non-whitespace character on the line!", parser.Token);

            // If the compiler directive is left-justified, set the format flag as such so that it will be displayed
            // at the left margin regardless of the current level of code indentation.
            if (parser.Token.LeadingWhitespace.Length == 0)
                SetFormatFlag(FormatFlags.NoIndentation, true);

            parser.NextToken();  // Move past '#'
        }

        /// <summary>
        /// The keyword associated with the compiler directive (if any).
        /// </summary>
        public virtual string DirectiveKeyword
        {
            get { return null; }
        }

        /// <summary>
        /// True if the compiler directive has an argument.
        /// </summary>
        public virtual bool HasArgument
        {
            get { return true; }  // Default is directive has an argument
        }

        /// <summary>
        /// Determines if the compiler directive should be indented.
        /// </summary>
        public virtual bool HasNoIndentationDefault
        {
            get { return true; }  // Default is no indentation for compiler directives
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return true; }
            set
            {
                if (!value)
                    throw new Exception("Can't set IsSingleLine to false for a CompilerDirective!");
                base.IsSingleLine = true;
            }
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            // Only associate regular comments with compiler directives, not doc comments
            return (comment is Comment);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            // Out-dent to far left if HasNoIndentation is true
            if (HasNoIndentation)
                writer.BeginOutdentOnNewLine(this, 0);

            // Compiler directives always start on a new line (but still check for IsPrefix)
            int newLines = NewLines;
            bool isPrefix = flags.HasFlag(RenderFlags.IsPrefix);
            if (!isPrefix && !flags.HasFlag(RenderFlags.SuppressNewLine))
                writer.WriteLines(newLines < 1 ? 1 : newLines);

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);
            UpdateLineCol(writer, flags);

            writer.Write(ParseToken);
            writer.Write(DirectiveKeyword);
            if (HasArgument)
            {
                writer.Write(" ");
                AsTextArgument(writer, passFlags);
            }

            AsTextEOLComments(writer, flags);
            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));

            // If this directive is rendered as a prefix, then the newline comes at the end
            if (isPrefix)
                writer.WriteLines(newLines < 1 ? 1 : newLines);
            else
                writer.NeedsNewLine = true;

            if (HasNoIndentation)
                writer.EndIndentation(this);
        }

        /// <summary>
        /// Determine a default of 1 or 2 newlines when adding items to a <see cref="Block"/>.
        /// </summary>
        public override int DefaultNewLines(CodeObject previous)
        {
            // Default to a preceeding blank line if the object has first-on-line annotations, or if
            // it's not also a CompilerDirective type.
            if (HasFirstOnLineAnnotations || !(previous is CompilerDirective))
                return 2;
            return 1;
        }

        protected virtual void AsTextArgument(CodeWriter writer, RenderFlags flags)
        { }

        protected virtual string ToStringDirective(RenderFlags flags)
        {
            return DirectiveKeyword;
        }
    }
}