using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Includes a conditional expression plus a body (a statement or block) that is repeatedly executed
    /// as long as the expression evaluates to true.  If <see cref="IsDoWhile"/> is true, the expression
    /// is evaluated at the bottom of the block ("do-while").
    /// </summary>
    /// <remarks>
    /// When <see cref="IsDoWhile"/> is true, an instance of <see cref="DoWhile"/> is used internally to
    /// represent the 'while' at the bottom of the loop, allowing for separate EOL comments and formatting
    /// from the 'do' part of the loop.  This object can be accessed with the <see cref="DoWhile"/> property.
    ///
    /// A "do { } while (true)" or "for (;;) { }" is equivalent to a "while (true) { }", and is converted to
    /// such upon parsing.  A "while (expr);" has a null body and HasTerminator is true.
    /// A "while (true) { }" statement is displayed as "do { }" in the GUI (an enhancement over standard C#).
    /// </remarks>
    public class While : BlockStatement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "while";

        /// <summary>
        /// The token used to parse a 'do' loop.
        /// </summary>
        public const string ParseTokenDo = "do";

        protected Expression _conditional;

        // A separate object is used for do-while loops so that the 'while' clause can have
        // separate EOL comments and formatting (IsFirstOnLine) from the 'do'.
        protected DoWhile _doWhile;

        /// <summary>
        /// Create a <see cref="While"/>.
        /// </summary>
        public While(Expression expression, bool isDoWhile, CodeObject body)
            : base(body, !isDoWhile)  // Don't allow null body for do-while
        {
            Conditional = expression;
            IsDoWhile = isDoWhile;
        }

        /// <summary>
        /// Create a <see cref="While"/>.
        /// </summary>
        public While(Expression expression, bool isDoWhile)
            : this(expression, isDoWhile, new Block())
        { }

        /// <summary>
        /// Create a <see cref="While"/>.
        /// </summary>
        public While(Expression expression, CodeObject body)
            : this(expression, false, body)
        { }

        /// <summary>
        /// Create a <see cref="While"/>.
        /// </summary>
        public While(Expression expression)
            : this(expression, false, new Block())
        { }

        /// <summary>
        /// Convert a For into a While.
        /// </summary>
        public While(For @for)
            : base(@for)
        {
            // In addition to the body, also move any conditional expression, and move any iteration expressions
            // into the end of the body.  Any Initializations are ignored - they should be manually moved to the
            // parent of the While.
            Conditional = @for.Conditional;
            if (@for.Iterations != null && @for.Iterations.Count > 0)
            {
                foreach (Expression expression in @for.Iterations)
                    Add(expression);
            }
        }

        protected While(Parser parser, CodeObject parent, bool isDoWhile)
                    : base(parser, parent)
        {
            if (isDoWhile)
            {
                parser.NextToken();                         // Move past 'do'
                new Block(out _body, parser, this, false);  // Parse the body

                // Parse optional 'while' child part
                if (parser.TokenText == ParseToken)
                    DoWhile = new DoWhile(parser, this);

                if (AutomaticCodeCleanup && !parser.IsGenerated)
                {
                    // Normalize 'do { } while (true)' to 'while (true)', and optimize to have a null condition (see below)
                    if (_conditional is Literal && ((Literal)_conditional).Text == Literal.ParseTokenTrue)
                    {
                        IsDoWhile = false;
                        _conditional = null;
                    }
                }
            }
            else
            {
                ParseKeywordArgumentBody(parser, ref _conditional, true, false);

                if (AutomaticCodeCleanup && !parser.IsGenerated)
                {
                    // Optimize "while (true)" to have a null conditional expression, which represents an infinite loop
                    // (and optionally allows it to display as a "do" without a "while" condition in the GUI).
                    if (_conditional is Literal && ((Literal)_conditional).Text == Literal.ParseTokenTrue)
                        _conditional = null;
                }
            }
        }

        /// <summary>
        /// The conditional <see cref="Expression"/>.
        /// </summary>
        public Expression Conditional
        {
            get { return _conditional; }
            set
            {
                SetField(ref _conditional, value, true);
                if (value == null)
                    IsDoWhile = false;
            }
        }

        /// <summary>
        /// The <see cref="DoWhile"/> part if this is a 'do/while' loop.
        /// </summary>
        public DoWhile DoWhile
        {
            get { return _doWhile; }
            set { SetField(ref _doWhile, value, false); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return (!IsDoWhile && HasCondition); }
        }

        /// <summary>
        /// True if the <see cref="BlockStatement"/> always requires braces.
        /// </summary>
        public override bool HasBracesAlways
        {
            get { return false; }
        }

        /// <summary>
        /// True if there is a conditional <see cref="Expression"/>.
        /// </summary>
        public bool HasCondition
        {
            get { return (_conditional != null); }
        }

        /// <summary>
        /// True if this is a 'do/while' loop.
        /// </summary>
        public bool IsDoWhile
        {
            get { return (_doWhile != null); }
            set
            {
                if (value && HasCondition)
                {
                    // Force to 'do-while' loop
                    if (_doWhile == null)
                        DoWhile = new DoWhile(this);
                }
                else
                {
                    // Force to 'while' loop
                    _doWhile = null;
                }
            }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_conditional == null || (!_conditional.IsFirstOnLine && _conditional.IsSingleLine))
                    && (_doWhile == null || (!_doWhile.IsFirstOnLine && _doWhile.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (value)
                {
                    if (_conditional != null)
                    {
                        _conditional.IsFirstOnLine = false;
                        _conditional.IsSingleLine = true;
                    }
                    if (_doWhile != null)
                    {
                        _doWhile.IsFirstOnLine = false;
                        _doWhile.IsSingleLine = true;
                    }
                }
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken; }
        }

        public static void AddParsePoints()
        {
            Parser.AddParsePoint(ParseToken, Parse, typeof(IBlock));
            Parser.AddParsePoint(ParseTokenDo, ParseDoWhile, typeof(IBlock));
        }

        /// <summary>
        /// Parse a <see cref="While"/>.
        /// </summary>
        public static While Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new While(parser, parent, false);
        }

        /// <summary>
        /// Parse a 'do/while' loop.
        /// </summary>
        public static While ParseDoWhile(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new While(parser, parent, true);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            While clone = (While)base.Clone();
            clone.CloneField(ref clone._conditional, _conditional);
            clone.CloneField(ref clone._doWhile, _doWhile);
            return clone;
        }

        protected override void AsTextAfter(CodeWriter writer, RenderFlags flags)
        {
            base.AsTextAfter(writer, flags);
            if (IsDoWhile && !flags.HasFlag(RenderFlags.Description))
                _doWhile.AsText(writer, flags | RenderFlags.PrefixSpace);
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            _conditional.AsText(writer, flags);
        }

        protected override void AsTextStatement(CodeWriter writer, RenderFlags flags)
        {
            UpdateLineCol(writer, flags);
            if (HasCondition)
                writer.Write(IsDoWhile ? ParseTokenDo : ParseToken);
            else
            {
                // Always render a null condition as "while (true)" in text
                writer.Write(ParseToken + " (true)");
            }
        }
    }
}