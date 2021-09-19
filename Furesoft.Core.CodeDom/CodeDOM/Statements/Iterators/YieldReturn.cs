using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Iterators.Base;
using Furesoft.Core.CodeDom.CodeDOM.Statements.Iterators;

namespace Furesoft.Core.CodeDom.CodeDOM.Statements.Iterators
{
    /// <summary>
    /// Yields the current value of an iterator.
    /// </summary>
    /// <remarks>
    /// The expression must not be null or empty.
    /// Can't appear anywhere in a 'try' that has any catches.
    /// Yield statements must only appear in method, operator, or accessor bodies.
    /// They must not appear in anonymous functions or a 'finally' clause.
    /// 'yield' isn't a reserved word - it has special meaning only before a 'return' or 'break'.
    /// </remarks>
    public class YieldReturn : YieldStatement
    {
        /// <summary>
        /// The second token used to parse the code object.
        /// </summary>
        public const string ParseToken2 = "return";

        protected Expression _expression;

        /// <summary>
        /// Create a <see cref="YieldReturn"/>.
        /// </summary>
        public YieldReturn(Expression expression)
        {
            Expression = expression;
        }

        /// <summary>
        /// Parse a <see cref="YieldReturn"/>.
        /// </summary>
        public YieldReturn(Parser parser, CodeObject parent)
            : base(parser, parent)
        {
            parser.NextToken();  // Move past 'yield'
            parser.NextToken();  // Move past 'return'
            SetField(ref _expression, Expression.Parse(parser, this, true), false);
            ParseTerminator(parser);
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
                if (value && _expression != null)
                {
                    _expression.IsFirstOnLine = false;
                    _expression.IsSingleLine = true;
                }
            }
        }

        /// <summary>
        /// The keyword associated with the <see cref="Statement"/>.
        /// </summary>
        public override string Keyword
        {
            get { return ParseToken1 + " " + ParseToken2; }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            YieldReturn clone = (YieldReturn)base.Clone();
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
