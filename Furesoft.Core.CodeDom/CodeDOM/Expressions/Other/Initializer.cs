using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Assignments;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Other
{
    /// <summary>
    /// Represents the initialization of an array with a list of <see cref="Expression"/>s.
    /// </summary>
    public class Initializer : Expression
    {
        /// <summary>
        /// The token used to parse the end of an initializer.
        /// </summary>
        public const string ParseTokenEnd = "}";

        /// <summary>
        /// The token used to parse the start of an initializer.
        /// </summary>
        public const string ParseTokenStart = "{";

        protected byte _endNewLines;
        protected ChildList<Expression> _expressions;

        /// <summary>
        /// Create an <see cref="Initializer"/>, with the optional child <see cref="Expression"/>s.
        /// </summary>
        public Initializer(params Expression[] expressions)
        {
            CreateExpressions().AddRange(expressions);
            foreach (Expression expression in expressions)
                expression.FormatAsArgument();
        }

        /// <summary>
        /// Parse an <see cref="Initializer"/>.
        /// </summary>
        public Initializer(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            MoveComments(parser.LastToken);  // Associate any skipped comment objects

            // If the initializer doesn't start on a new line, or it's indented less than the parent object, set the NoIndentation
            // flag to prevent it from being formatted relative to the parent object.
            if (!IsFirstOnLine || parser.CurrentTokenIndentedLessThan(parser.ParentStartingToken))
                SetFormatFlag(FormatFlags.NoIndentation, true);

            parser.NextToken();  // Move past '{'
            Token lastToken = parser.LastToken;
            MoveEOLCommentAsInfix(lastToken);

            // Parse the list of expressions
            _expressions = ParseList(parser, this, ParseTokenEnd);

            // Attach any skipped regular comment to the first item in the list
            if (_expressions != null && _expressions.Count > 0)
                _expressions[0].MoveComments(lastToken);

            if (ParseExpectedToken(parser, ParseTokenEnd))  // Move past '}'
            {
                EndNewLines = parser.LastToken.NewLines;  // Set the newline count for the '}'
                MoveEOLComment(parser.LastToken);
            }
        }

        /// <summary>
        /// The number of newlines preceeding the closing '}' (0 to N).
        /// </summary>
        public int EndNewLines
        {
            get { return _endNewLines; }
            set { _endNewLines = (byte)value; }
        }

        /// <summary>
        /// A collection of child <see cref="Expression"/>s.
        /// </summary>
        public ChildList<Expression> Expressions
        {
            get { return _expressions; }
        }

        /// <summary>
        /// True if there are any child <see cref="Expression"/>s.
        /// </summary>
        public bool HasExpressions
        {
            get { return (_expressions != null && _expressions.Count > 0); }
        }

        /// <summary>
        /// Always <c>false</c>.
        /// </summary>
        public override bool HasTerminator
        {
            // Intializers don't have terminators (any terminator will belong to the parent), so disable use of this flag
            get { return false; }
            set { }
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
        /// True if the closing paren or bracket is on a new line.
        /// </summary>
        public override bool IsEndFirstOnLine
        {
            get { return (_endNewLines > 0); }
            set
            {
                if (value)
                {
                    if (_endNewLines == 0)
                        _endNewLines = 1;
                }
                else
                    _endNewLines = 0;
            }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_expressions == null || _expressions.Count == 0 || (!_expressions[0].IsFirstOnLine && _expressions.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;

                // For Initializers, NewLines is the number of newlines before the '{', and EndNewLines
                // is the number of newlines before the '}'.
                NewLines = (value ? 0 : 1);
                EndNewLines = (value ? 0 : 1);

                if (_expressions != null && _expressions.Count > 0)
                {
                    _expressions[0].IsFirstOnLine = !value;
                    _expressions.IsSingleLine = value;
                }
            }
        }

        public static new void AddParsePoints()
        {
            // Use a parse-priority of 400 (GenericMethodDecl uses 0, UnresolvedRef uses 100, PropertyDeclBase uses 200, BlockDecl uses 300)
            Parser.AddParsePoint(ParseTokenStart, 400, Parse);
        }

        /// <summary>
        /// Parse an <see cref="Initializer"/>.
        /// </summary>
        public static Initializer Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Initializer(parser, parent);
        }

        public override void AsText(CodeWriter writer, RenderFlags flags)
        {
            // This method is overridden for the special indentation logic, and while we're at it,
            // the isPrefix and hasParens logic is left out.
            int newLines = NewLines;
            if (newLines > 0)
            {
                if (!flags.HasFlag(RenderFlags.SuppressNewLine))
                    writer.WriteLines(newLines);
            }
            else if (flags.HasFlag(RenderFlags.PrefixSpace))
                writer.Write(" ");

            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            AsTextBefore(writer, passFlags | RenderFlags.IsPrefix);

            // Make the indentation of the Initializer relative to the open brace column if the brace is
            // on a line by itself.  Otherwise, do a normal indent if requested.
            bool increaseIndent = false;
            bool isMultiLine = (IsFirstOnLine && _expressions != null && _expressions[0].IsFirstOnLine);
            if (isMultiLine)
                writer.BeginIndentOnNewLineRelativeToCurrentOffset(this);
            else
            {
                // Increase the indent level for any newlines that occur within the initializer if the flag is set
                increaseIndent = flags.HasFlag(RenderFlags.IncreaseIndent);
                if (increaseIndent)
                    writer.BeginIndentOnNewLine(this);
            }

            AsTextExpression(writer, passFlags | (flags & (RenderFlags.Attribute | RenderFlags.HasDotPrefix | RenderFlags.Declaration)));
            if (HasTerminator)
                writer.Write(Statement.ParseTokenTerminator);
            AsTextEOLComments(writer, flags);

            if (isMultiLine || increaseIndent)
                writer.EndIndentation(this);

            AsTextAfter(writer, passFlags | (flags & RenderFlags.NoPostAnnotations));
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            // Set the parent position for any post-comments inside the Initializer
            writer.SetParentOffset();
            UpdateLineCol(writer, flags);

            // Render the open brace
            writer.Write(ParseTokenStart);
            flags &= ~RenderFlags.SuppressNewLine;

            if (!flags.HasFlag(RenderFlags.NoEOLComments))
                AsTextInfixEOLComments(writer, flags);

            // Get any column widths calculated by a parent Initializer, or calculate them
            int[] columnWidths = null, nestedColumnWidths = null;
            if (Parent is Initializer)
                columnWidths = writer.GetColumnWidths(Parent);
            if (columnWidths == null)
            {
                nestedColumnWidths = CalculateNestedColumnWidths();

                // If we have alignments for nested Initializers, create an alignment state to hold them for later
                if (nestedColumnWidths != null)
                    writer.BeginAlignment(this, nestedColumnWidths);

                // Now that we've set the nested column widths, we can properly calculate the width of the parent
                columnWidths = CalculateColumnWidths(writer);
            }

            // Render the body of the initializer - always prefix spaces, even on the first item after the '{'.
            // Pass any column widths through for formatting.
            writer.WriteList(_expressions, flags | RenderFlags.PrefixSpace, this, columnWidths);

            if (nestedColumnWidths != null)
                writer.EndAlignment(this);

            // Render the close brace
            int endNewLines = EndNewLines;
            if (endNewLines > 0)
                writer.WriteLines(endNewLines);
            else
                writer.Write(" ");
            writer.Write(ParseTokenEnd);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Initializer clone = (Initializer)base.Clone();
            clone._expressions = ChildListHelpers.Clone(_expressions, clone);
            return clone;
        }

        /// <summary>
        /// Create the list of <see cref="Expression"/>s, or return the existing one.
        /// </summary>
        /// <returns></returns>
        public ChildList<Expression> CreateExpressions()
        {
            if (_expressions == null)
                _expressions = new ChildList<Expression>(this);
            return _expressions;
        }

        protected int[] CalculateColumnWidths(CodeWriter writer)
        {
            // Check for alignment of Initializer members into columns
            List<int> columnWidths = new() { 0 };
            if (_expressions != null && _expressions.Count > 1)
            {
                int column = 0;
                bool isFirst = true;
                bool multiLine = false;
                int lineLength = 0;
                int maxLineLength = 0;
                foreach (Expression expression in _expressions)
                {
                    // Determine the current column, handling line wraps
                    if (expression.IsFirstOnLine)
                    {
                        column = 0;
                        if (!isFirst)
                            multiLine = true;
                        if (lineLength > maxLineLength)
                            maxLineLength = lineLength;
                        lineLength = 0;
                    }
                    else
                    {
                        if (++column > columnWidths.Count - 1)
                            columnWidths.Add(0);
                    }

                    // Don't align if there is a newline in a multi-column list.  Also, don't align object initializers for now.
                    Comment postfixComment = expression.GetComment(delegate (Comment comment) { return comment.IsPostfix; });
                    if ((multiLine && postfixComment != null && postfixComment.NewLines > 1 && columnWidths.Count > 1) || expression is Assignment)
                    {
                        columnWidths = null;
                        break;
                    }

                    int length = expression.AsTextLength(RenderFlags.LengthFlags, writer.AlignmentStateStack);
                    if (length > columnWidths[column])
                        columnWidths[column] = length;
                    lineLength += length + 2;  // Calculate approximate line length, including ", "
                    isFirst = false;
                }
                if (columnWidths != null)
                {
                    // Abort alignment if not multi-line, or if the alignment exceeds the max column *and* increases the width by more than 20%
                    int alignmentWidth = Enumerable.Sum(columnWidths, delegate (int width) { return width + 2; });
                    if (!multiLine || (alignmentWidth > MaximumLineLength && (double)alignmentWidth / maxLineLength > 1.2))
                        columnWidths = null;
                }
            }

            return (columnWidths != null ? columnWidths.ToArray() : null);
        }

        protected int[] CalculateNestedColumnWidths()
        {
            // Format tables made of nested Initializers into columns if possible
            List<int> columnWidths = null;
            if (_expressions != null && _expressions.Count > 1)
            {
                foreach (Expression expression in _expressions)
                {
                    if (expression is Initializer initializer)
                    {
                        if (!initializer.IsEndFirstOnLine)
                        {
                            ChildList<Expression> expressions = initializer.Expressions;
                            if (expressions != null && expressions.IsSingleLine)
                            {
                                int column = -1;
                                if (columnWidths == null)
                                    columnWidths = new List<int>();
                                foreach (Expression innerExpression in expressions)
                                {
                                    if (++column > columnWidths.Count - 1)
                                        columnWidths.Add(0);
                                    int length = innerExpression.AsTextLength();
                                    if (length > columnWidths[column])
                                        columnWidths[column] = length;
                                }
                            }
                        }
                    }
                }
            }
            return (columnWidths != null ? columnWidths.ToArray() : null);
        }
    }
}