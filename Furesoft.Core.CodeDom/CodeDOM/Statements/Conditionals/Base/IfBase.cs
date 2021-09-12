using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Conditionals.Base
{
    /// <summary>
    /// The common base class of <see cref="If"/> and <see cref="ElseIf"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="If"/> or <see cref="ElseIf"/> statement represents conditional flow control, and includes a conditional
    /// expression, a body, and an optional <see cref="Else"/> or <see cref="ElseIf"/> statement.
    /// </remarks>
    public class IfBase : BlockStatement
    {
        #region /* FIELDS */

        protected Expression _conditional;
        protected BlockStatement _elsePart;  // Must be an Else or an ElseIf

        #endregion

        #region /* CONSTRUCTORS */

        protected IfBase(Expression conditional, CodeObject body)
            : base(body, false)  // Don't allow null body for If/ElseIf
        {
            Conditional = conditional;
        }

        protected IfBase(Expression conditional, CodeObject body, Else @else)
            : this(conditional, body)
        {
            ElsePart = @else;
        }

        protected IfBase(Expression conditional, CodeObject body, ElseIf elseIf)
            : this(conditional, body)
        {
            ElsePart = elseIf;
        }

        protected IfBase(Expression conditional)
            : this(conditional, (CodeObject)null)
        { }

        protected IfBase(Expression conditional, Else @else)
            : this(conditional, null, @else)
        { }

        protected IfBase(Expression conditional, ElseIf elseIf)
            : this(conditional, null, elseIf)
        { }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The conditional <see cref="Expression"/>.
        /// </summary>
        public Expression Conditional
        {
            get { return _conditional; }
            set { SetField(ref _conditional, value, true); }
        }

        /// <summary>
        /// The optional <see cref="ElseIf"/> or <see cref="Else"/> part.
        /// </summary>
        public BlockStatement ElsePart
        {
            get { return _elsePart; }
            set { SetField(ref _elsePart, value, false); }
        }

        /// <summary>
        /// True if there is an <see cref="ElseIf"/> or <see cref="Else"/> part.
        /// </summary>
        public bool HasElse
        {
            get { return (_elsePart != null); }
        }

        /// <summary>
        /// True for multi-part statements, such as try/catch/finally or if/else.
        /// </summary>
        public override bool IsMultiPart
        {
            get { return HasElse; }
        }

        #endregion

        #region /* METHODS */

        protected override bool IsChildIndented(CodeObject obj)
        {
            // The child object can only be indented if it's the first thing on the line
            if (obj.IsFirstOnLine)
            {
                // If the child isn't an else part and isn't a prefix, it should be indented
                // (we can't compare to _elsePart because it won't be set yet if we're still
                // parsing the child, so just check for Else and ElseIf objects instead).
                return (!(obj is Else || obj is ElseIf) && !IsChildPrefix(obj));
            }
            return false;
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            IfBase clone = (IfBase)base.Clone();
            clone.CloneField(ref clone._conditional, _conditional);
            clone.CloneField(ref clone._elsePart, _elsePart);
            return clone;
        }

        #endregion

        #region /* PARSING */

        protected IfBase(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        protected void ParseIf(Parser parser, CodeObject parent)
        {
            // Flush any unused objects first, so that they don't interfere with skipping
            // compiler directives below, or the parsing of any 'else' part.
            if (parser.HasUnused && _parent is BlockStatement)
                ((BlockStatement)_parent).Body.FlushUnused(parser);

            ParseKeywordArgumentBody(parser, ref _conditional, false, false);  // Parse keyword, argument, and body

            // Skip over any compiler directives that might occur before an 'else', adding them to the unused list
            ParseAnnotations(parser, parent, false, true);

            // Parse optional 'else' or 'else if' child part
            if (parser.TokenText == Else.ParseToken)
            {
                _elsePart = ElseIf.Parse(parser, this);
                if (_elsePart == null)
                    _elsePart = Else.Parse(parser, this);
            }
            else
            {
                // If there wasn't any 'else', and we skipped any compiler directives, we have to move
                // them now so they can be manually flushed by the parent Block *after* this statement.
                if (parser.HasUnused)
                    parser.MoveUnusedToPostUnused();
            }
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            _conditional = (Expression)_conditional.Resolve(ResolveCategory.Expression, flags);
            base.Resolve(ResolveCategory.CodeObject, flags);
            if (HasElse)
                _elsePart.Resolve(ResolveCategory.CodeObject, flags);
            return this;
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_conditional != null && _conditional.HasUnresolvedRef())
                return true;
            if (_elsePart != null && _elsePart.HasUnresolvedRef())
                return true;
            return base.HasUnresolvedRef();
        }

        #endregion

        #region /* FORMATTING */

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return true; }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
        {
            get { return false; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_conditional == null || (!_conditional.IsFirstOnLine && _conditional.IsSingleLine))
                    && (_elsePart == null || (!_elsePart.IsFirstOnLine && _elsePart.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_conditional != null)
                {
                    if (value)
                        _conditional.IsFirstOnLine = false;
                    _conditional.IsSingleLine = value;
                }
                if (_elsePart != null)
                {
                    _elsePart.IsFirstOnLine = !value;
                    _elsePart.IsSingleLine = value;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_conditional != null)
            {
                // Technically, we should indent all expressions relative to their starting position.  However, this
                // would be a big deviation from standard formatting, and so would have to be optional.  We'd have to
                // wrap all statements with expressions like this:
                //writer.BeginIndentOnNewLineRelativeToCurrentPosition(this);
                _conditional.AsText(writer, flags);
                //writer.EndIndentation(this);
            }
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextAfter(writer, flags);
            if (HasElse && !flags.HasFlag(RenderFlags.Description))
                _elsePart.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        #endregion
    }
}
