using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Miscellaneous
{
    /// <summary>
    /// Maintains a critical section on the provided object while the body of the statement is executing.
    /// </summary>
    public class Lock : BlockStatement
    {
        #region /* FIELDS */

        protected Expression _target;

        #endregion

        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a <see cref="Lock"/>.
        /// </summary>
        public Lock(Expression target, CodeObject body)
            : base(body, false)
        {
            Target = target;
        }

        /// <summary>
        /// Create a <see cref="Lock"/>.
        /// </summary>
        public Lock(Expression target)
            : base(null, false)
        {
            Target = target;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The target <see cref="Expression"/> of the <see cref="Lock"/>.
        /// </summary>
        public Expression Target
        {
            get { return _target; }
            set { SetField(ref _target, value, true); }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Lock clone = (Lock)base.Clone();
            clone.CloneField(ref clone._target, _target);
            return clone;
        }

        #endregion

        #region /* PARSING */

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "lock";

        internal static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="Lock"/>.
        /// </summary>
        public static Lock Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Lock(parser, parent);
        }

        protected Lock(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseKeywordArgumentBody(parser, ref _target, false, false);
        }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            _target = (Expression)_target.Resolve(ResolveCategory.Expression, flags);
            return base.Resolve(ResolveCategory.CodeObject, flags);
        }

        /// <summary>
        /// Returns true if the code object is an <see cref="UnresolvedRef"/> or has any <see cref="UnresolvedRef"/> children.
        /// </summary>
        public override bool HasUnresolvedRef()
        {
            if (_target != null && _target.HasUnresolvedRef())
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
            get { return (base.IsSingleLine && (_target == null || (!_target.IsFirstOnLine && _target.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (value && _target != null)
                {
                    _target.IsFirstOnLine = false;
                    _target.IsSingleLine = true;
                }
            }
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            _target.AsText(writer, flags);
        }

        #endregion
    }
}
