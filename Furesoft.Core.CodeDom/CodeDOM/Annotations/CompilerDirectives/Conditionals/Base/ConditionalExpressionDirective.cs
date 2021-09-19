using Furesoft.Core.CodeDom.Rendering;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Nova.CodeDOM;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/> and <see cref="ElIfDirective"/>.
    /// </summary>
    public abstract class ConditionalExpressionDirective : ConditionalDirective
    {
        // Conditional directive expressions can only use DirectiveSymbolRefs, 'true' and 'false'
        // literals, and these operators: &&, ||, !, ==, !=
        protected Expression _expression;

        protected ConditionalExpressionDirective(Expression expression)
        {
            Expression = expression;

            // Force off parens for conditional directive expressions
            if (expression != null)
                expression.HasParens = false;
        }

        protected ConditionalExpressionDirective(Parser parser, CodeObject parent)
                    : base(parser, parent)
        {
            Token token = parser.NextTokenSameLine(false);  // Move past 'if' or 'elif'
            if (token != null)
                SetField(ref _expression, Expression.ParseDirectiveExpression(parser, this), false);

            // Turn off any parens on the expression, since they're not necessary
            if (AutomaticFormattingCleanup && !parser.IsGenerated && _expression != null && _expression.HasParens)
                _expression.HasParens = false;

            // Move any EOL comment on the expression to the conditional directive instead
            MoveEOLComment(_expression);
            Expression expression = _expression;
            while (expression is BinaryOperator)
            {
                MoveEOLComment(expression);
                expression = ((BinaryOperator)expression).Right;
            }

            // Skip the next section of code if an earlier 'if' or 'elif' evaluated to true, or
            // if this one doesn't evaluate to true.
            bool eval = false;
            if (_expression is DirectiveSymbolRef)
                eval = FindParent<CodeUnit>().IsCompilerDirectiveSymbolDefined(((DirectiveSymbolRef)_expression).Name);
            else if (_expression is Literal)
                eval = ((Literal)_expression).Text == Literal.ParseTokenTrue;
            if (parser.CurrentConditionalDirectiveState || !eval)
                SkipSection(parser);
            else
                parser.CurrentConditionalDirectiveState = _isActive = true;
        }

        /// <summary>
        /// The conditional <see cref="Expression"/> of the directive.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ConditionalExpressionDirective clone = (ConditionalExpressionDirective)base.Clone();
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
