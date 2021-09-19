using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Maintains a critical section on the provided object while the body of the statement is executing.
    /// </summary>
    public class Lock : BlockStatement
    {
        protected Expression _target;

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

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        /// <summary>
        /// The target <see cref="Expression"/> of the <see cref="Lock"/>.
        /// </summary>
        public Expression Target
        {
            get { return _target; }
            set { SetField(ref _target, value, true); }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Lock clone = (Lock)base.Clone();
            clone.CloneField(ref clone._target, _target);
            return clone;
        }

        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "lock";

        protected Lock(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            ParseKeywordArgumentBody(parser, ref _target, false, false);
        }

        public static void AddParsePoints()
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

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            _target.AsText(writer, flags);
        }
    }
}