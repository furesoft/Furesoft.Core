using Furesoft.Core.CodeDom.Rendering;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Represents a list of one or more unrecognized identifiers or symbols (UnresolvedRefs).
    /// If it's part of a larger expression (as opposed to being a stand-alone expression), then the last entry in the list
    /// will itself be a valid child expression.
    /// </summary>
    public class Unrecognized : Expression
    {
        /// <summary>
        /// The total number of <see cref="Unrecognized"/> objects.
        /// </summary>
        public static int Count;

        protected ChildList<Expression> _expressions;
        protected bool _inDocComment;
        protected bool _parsingBlock;

        /// <summary>
        /// Create an <see cref="Unrecognized"/> object.
        /// </summary>
        public Unrecognized(bool parsingBlock, bool inDocComment, params Expression[] expressions)
        {
            // Only count unrecognized "real" code - ignore any inside doc comments
            if (!inDocComment)
                ++Count;

            _parsingBlock = parsingBlock;
            _inDocComment = inDocComment;
            _expressions = new ChildList<Expression>(this);
            foreach (Expression expression in expressions)
                AddRight(expression);
        }

        /// <summary>
        /// The column number of the first contained <see cref="Expression"/>.
        /// </summary>
        public override int ColumnNumber
        {
            get { return _expressions[0].ColumnNumber; }
        }

        /// <summary>
        /// A collection of child <see cref="Expression"/>s.
        /// </summary>
        public ChildList<Expression> Expressions
        {
            get { return _expressions; }
        }

        /// <summary>
        /// True if inside a <see cref="DocComment"/>.
        /// </summary>
        public bool InDocComment
        {
            get { return _inDocComment; }
        }

        /// <summary>
        /// The line number of the first contained <see cref="Expression"/>.
        /// </summary>
        public override int LineNumber
        {
            get { return _expressions[0].LineNumber; }
        }

        /// <summary>
        /// Add an <see cref="Expression"/> to the left side.
        /// </summary>
        public void AddLeft(Expression expression)
        {
            if (expression != null)
            {
                // Move any newlines on the parent back to first item in the list, and
                // then move any newlines on the new item to the parent.
                if (_expressions.Count > 0)
                    _expressions[0].MoveFormatting(this);
                MoveFormatting(expression);

                // Clear the current parent first, to prevent a Clone() of the object, so that any
                // notified error messages don't end up pointing to an orphaned original object.
                expression.Parent = null;
                _expressions.Insert(0, expression);
            }
        }

        /// <summary>
        /// Add an <see cref="Expression"/> to the right side.
        /// </summary>
        public void AddRight(Expression expression)
        {
            if (expression != null)
            {
                // Move any newlines to the parent
                if (_expressions.Count == 0 && expression.IsFirstOnLine)
                    MoveFormatting(expression);

                // Clear the current parent first, to prevent a Clone() of the object, so that any
                // notified error messages don't end up pointing to an orphaned original object.
                expression.Parent = null;
                _expressions.Add(expression);
            }
        }

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return false;
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            UpdateLineCol(writer, flags);
            writer.WriteList(_expressions, passFlags | RenderFlags.NoItemSeparators, this);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Unrecognized clone = (Unrecognized)base.Clone();
            clone._expressions = ChildListHelpers.Clone(_expressions, clone);
            return clone;
        }

        /// <summary>
        /// Update the attached error message with the current content.
        /// </summary>
        public void UpdateMessage()
        {
            RemoveAllMessages(MessageSource.Parse);
            if (_inDocComment)
            {
                AttachMessage("Unrecognized code in doc comment: '" + AsString() + "'"
                    + (_parsingBlock ? " - use '<c></c>' for code fragments (expressions)" : ""),
                    MessageSeverity.Warning, MessageSource.Parse);
            }
            else
                AttachMessage("UNRECOGNIZED CODE: '" + AsString() + "'", MessageSeverity.Error, MessageSource.Parse);
        }
    }
}