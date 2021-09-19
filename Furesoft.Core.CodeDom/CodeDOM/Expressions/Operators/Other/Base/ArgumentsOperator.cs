using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all operators with a variable list of arguments (<see cref="Call"/>, <see cref="Index"/>,
    /// <see cref="NewOperator"/>).
    /// </summary>
    public abstract class ArgumentsOperator : Operator
    {
        protected ChildList<Expression> _arguments;
        protected Expression _expression;

        protected ArgumentsOperator(Expression expression, params Expression[] arguments)
        {
            Expression = expression;
            if (arguments != null && arguments.Length > 0)
            {
                CreateArguments().AddRange(arguments);
                foreach (Expression argument in arguments)
                {
                    // Arguments can be null for NewArray
                    if (argument != null)
                        argument.FormatAsArgument();
                }
            }
        }

        protected ArgumentsOperator(Parser parser, CodeObject parent)
                    : base(parser, parent)
        { }

        /// <summary>
        /// The number of arguments.
        /// </summary>
        public int ArgumentCount
        {
            get { return (_arguments != null ? _arguments.Count : 0); }
        }

        /// <summary>
        /// The argument expressions.
        /// </summary>
        public ChildList<Expression> Arguments
        {
            get { return _arguments; }
        }

        /// <summary>
        /// The <see cref="Expression"/> being invoked.
        /// </summary>
        public virtual Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// True if there are any arguments.
        /// </summary>
        public bool HasArguments
        {
            get { return (_arguments != null && _arguments.Count > 0); }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get
            {
                return (base.IsSingleLine && (_expression == null || (!_expression.IsFirstOnLine && _expression.IsSingleLine))
                    && (_arguments == null || _arguments.Count == 0 || ((_arguments[0] == null || !_arguments[0].IsFirstOnLine) && _arguments.IsSingleLine)));
            }
            set
            {
                base.IsSingleLine = value;
                if (_expression != null)
                {
                    if (value)
                        _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = value;
                }
                if (_arguments != null && _arguments.Count > 0)
                {
                    if (value)
                        _arguments[0].IsFirstOnLine = false;
                    _arguments.IsSingleLine = value;
                }
            }
        }

        /// <summary>
        /// Add one or more <see cref="Expression"/>s.
        /// </summary>
        public void AddArguments(params Expression[] expressions)
        {
            CreateArguments().AddRange(expressions);
        }

        public override void AsTextExpression(CodeWriter writer, RenderFlags flags)
        {
            RenderFlags passFlags = (flags & RenderFlags.PassMask);
            bool attributeCall = flags.HasFlag(RenderFlags.Attribute);

            // Get the parent position and wrap the name/arguments in its own indent logic to preserve
            // the parent position until the initializer is rendered.
            writer.SetParentOffset();
            writer.BeginIndentOnNewLine(this);

            AsTextName(writer, passFlags | (flags & RenderFlags.Attribute));  // Special case - allow the Attribute flag to pass
            if ((_arguments != null && _arguments.Count > 0) || (!flags.HasFlag(RenderFlags.NoParensIfEmpty) && !attributeCall) || HasInfixComments)
            {
                writer.BeginIndentOnNewLineRelativeToLastIndent(this, _expression);
                AsTextStartArguments(writer, flags);
                AsTextInfixComments(writer, 0, flags);
                writer.WriteList(_arguments, passFlags, this);
                AsTextEndArguments(writer, flags);
                writer.EndIndentation(this);
            }

            writer.EndIndentation(this);
            AsTextInitializer(writer, flags);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ArgumentsOperator clone = (ArgumentsOperator)base.Clone();
            clone._arguments = ChildListHelpers.Clone(_arguments, clone);
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        /// <summary>
        /// Create the list of argument <see cref="Expression"/>s, or return the existing one.
        /// </summary>
        public ChildList<Expression> CreateArguments()
        {
            if (_arguments == null)
                _arguments = new ChildList<Expression>(this);
            return _arguments;
        }

        /// <summary>
        /// Determine the type of the parameter for the specified argument index.
        /// </summary>
        public virtual TypeRefBase GetParameterType(int argumentIndex)
        {
            return null;
        }

        protected abstract void AsTextEndArguments(CodeWriter writer, RenderFlags flags);

        protected virtual void AsTextInitializer(CodeWriter writer, RenderFlags flags)
        { }

        protected abstract void AsTextName(CodeWriter writer, RenderFlags flags);

        protected abstract void AsTextStartArguments(CodeWriter writer, RenderFlags flags);

        protected void ParseArguments(Parser parser, CodeObject parent, string parseTokenStart, string parseTokenEnd, bool allowSingleNullArgument)
        {
            Token openParenToken = parser.Token;
            if (!ParseExpectedToken(parser, parseTokenStart))  // Move past '(' (or '[')
                openParenToken = null;

            // Parse the list of argument expressions with our parent set to block bubble-up normalization of EOL comments.
            // This also handles proper parsing of nested Conditional expressions (resetting tracking for call arguments).
            parser.PushNormalizationBlocker(parent);
            _arguments = ParseList(parser, this, parseTokenEnd, ParseFlags.Arguments, allowSingleNullArgument);
            parser.PopNormalizationBlocker();

            if (openParenToken != null)
            {
                // Move any newlines on the open paren (or bracket) to the first argument instead, or just remove them
                // if the first argument is null (such as in '[]').
                if (_arguments != null && _arguments.Count > 0 && _arguments[0] != null)
                    _arguments[0].MoveFormatting(openParenToken);
                else if (openParenToken.IsFirstOnLine)
                    openParenToken.NewLines = 0;

                Token lastToken = parser.LastToken;
                if (ParseExpectedToken(parser, parseTokenEnd))  // Move past ')' (or ']')
                {
                    IsEndFirstOnLine = parser.LastToken.IsFirstOnLine;
                    if (_arguments == null || _arguments.Count == 0)
                        parent.MoveAllComments(lastToken, false, false, AnnotationFlags.IsInfix1);
                    parent.MoveEOLComment(parser.LastToken);  // Associate any skipped EOL comment with the parent
                }
            }
        }

        protected void ParseArguments(Parser parser, CodeObject parent, string parseTokenStart, string parseTokenEnd)
        {
            ParseArguments(parser, parent, parseTokenStart, parseTokenEnd, false);
        }
    }
}