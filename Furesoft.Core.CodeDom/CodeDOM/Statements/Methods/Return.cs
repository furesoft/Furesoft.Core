using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base.Interfaces;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other;

namespace Nova.CodeDOM
{
    /// <summary>
    /// Indicates that execution should return from the current method, and has an optional <see cref="Expression"/>
    /// to be evaluated as the return value.
    /// </summary>
    public class Return : Statement
    {
        /// <summary>
        /// The token used to parse the code object.
        /// </summary>
        public const string ParseToken = "return";

        protected Expression _expression;

        /// <summary>
        /// Create a <see cref="Return"/>.
        /// </summary>
        public Return(Expression expression)
        {
            Expression = expression;

            // Force parens on any binary operator expression with precedence greater than 200, or the Conditional operator
            if ((expression is BinaryOperator && ((BinaryOperator)expression).GetPrecedence() > 200) || expression is Conditional)
                expression.HasParens = true;
        }

        /// <summary>
        /// Create a <see cref="Return"/>.
        /// </summary>
        public Return()
            : this(null)
        { }

        protected Return(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            parser.NextToken();  // Move past 'return'
            SetField(ref _expression, Expression.Parse(parser, this, true), false);
            ParseTerminator(parser);

            // Get rid of parens around the expression if they serve no purpose
            if (AutomaticFormattingCleanup && !parser.IsGenerated && _expression != null
                && !((_expression is BinaryOperator && ((BinaryOperator)_expression).GetPrecedence() > 200) || _expression is Conditional))
                _expression.HasParens = false;
        }

        /// <summary>
        /// The return <see cref="Expression"/>.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has an argument.
        /// </summary>
        public override bool HasArgument
        {
            get { return (_expression != null); }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has parens around its argument.
        /// </summary>
        public override bool HasArgumentParens
        {
            get { return false; }
        }

        /// <summary>
        /// True if the <see cref="Statement"/> has a terminator character by default.
        /// </summary>
        public override bool HasTerminatorDefault
        {
            get { return true; }
        }

        /// <summary>
        /// Determines if the code object only requires a single line for display.
        /// </summary>
        public override bool IsSingleLine
        {
            get { return (base.IsSingleLine && (_expression == null || (!_expression.IsFirstOnLine && _expression.IsSingleLine))); }
            set
            {
                base.IsSingleLine = value;
                if (_expression != null)
                {
                    if (value)
                        _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = value;
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
        }

        /// <summary>
        /// Parse a <see cref="Return"/>.
        /// </summary>
        public static Return Parse(Parser parser, CodeObject parent, ParseFlags flags)
        {
            return new Return(parser, parent);
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            Return clone = (Return)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_expression != null)
                _expression.AsText(writer, flags);
        }
    }
}