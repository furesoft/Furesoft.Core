using Furesoft.Core.CodeDom.CodeDOM.Annotations.Comments.Base;
using Furesoft.Core.CodeDom.CodeDOM.Annotations;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Other;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Other
{
    /// <summary>
    /// Represents a list of one or more unrecognized identifiers or symbols (UnresolvedRefs).
    /// If it's part of a larger expression (as opposed to being a stand-alone expression), then the last entry in the list
    /// will itself be a valid child expression.
    /// </summary>
    public class Unrecognized : Expression
    {
        #region /* FIELDS */

        protected bool _parsingBlock;
        protected bool _inDocComment;
        protected ChildList<Expression> _expressions;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// The total number of <see cref="Unrecognized"/> objects.
        /// </summary>
        public static int Count;

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

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// True if inside a <see cref="DocComment"/>.
        /// </summary>
        public bool InDocComment
        {
            get { return _inDocComment; }
        }

        /// <summary>
        /// A collection of child <see cref="Expression"/>s.
        /// </summary>
        public ChildList<Expression> Expressions
        {
            get { return _expressions; }
        }

        /// <summary>
        /// The line number of the first contained <see cref="Expression"/>.
        /// </summary>
        public override int LineNumber
        {
            get { return _expressions[0].LineNumber; }
        }

        /// <summary>
        /// The column number of the first contained <see cref="Expression"/>.
        /// </summary>
        public override int ColumnNumber
        {
            get { return _expressions[0].ColumnNumber; }
        }

        #endregion

        #region /* METHODS */

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

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Unrecognized clone = (Unrecognized)base.Clone();
            clone._expressions = ChildListHelpers.Clone(_expressions, clone);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// Determine if the specified comment should be associated with the current code object during parsing.
        /// </summary>
        public override bool AssociateCommentWhenParsing(CommentBase comment)
        {
            return false;
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            ChildListHelpers.Resolve(_expressions, resolveCategory, flags);
            return this;
        }

        /// <summary>
        /// Evaluate the type of the <see cref="Expression"/>.
        /// </summary>
        /// <returns>The resulting <see cref="TypeRef"/> or <see cref="UnresolvedRef"/>.</returns>
        public override TypeRefBase EvaluateType(bool withoutConstants)
        {
            // Treat unrecognized expressions as 'object' type
            return TypeRef.ObjectRef;
        }

        #endregion

        #region /* FORMATTING */

        #endregion

        #region /* RENDERING */

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            UpdateLineCol(writer, flags);
            writer.WriteList(_expressions, passFlags | RenderFlags.NoItemSeparators, this);
        }

        #endregion
    }
}
