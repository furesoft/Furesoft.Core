using Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.References.Types;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Rendering;

namespace Furesoft.Core.CodeDom.CodeDOM.Annotations.CompilerDirectives.Conditionals.Base
{
    /// <summary>
    /// The common base class of <see cref="IfDirective"/> and <see cref="ElIfDirective"/>.
    /// </summary>
    public abstract class ConditionalExpressionDirective : ConditionalDirective
    {
        #region /* FIELDS */

        // Conditional directive expressions can only use DirectiveSymbolRefs, 'true' and 'false'
        // literals, and these operators: &&, ||, !, ==, !=
        protected Expression _expression;

        #endregion

        #region /* CONSTRUCTORS */

        protected ConditionalExpressionDirective(Expression expression)
        {
            Expression = expression;

            // Force off parens for conditional directive expressions
            if (expression != null)
                expression.HasParens = false;
        }

        #endregion

        #region /* PROPERTIES */

        /// <summary>
        /// The conditional <see cref="Expression"/> of the directive.
        /// </summary>
        public Expression Expression
        {
            get { return _expression; }
            set { SetField(ref _expression, value, true); }
        }

        #endregion

        #region /* METHODS */

        /// <summary>
        /// Deep-clone the code object.
        /// </summary>
        public override CodeObject Clone()
        {
            ConditionalExpressionDirective clone = (ConditionalExpressionDirective)base.Clone();
            clone.CloneField(ref clone._expression, _expression);
            return clone;
        }

        #endregion

        #region /* PARSING */

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
            TypeRef typeRef = (_expression != null ? _expression.EvaluateType() as TypeRef : null);
            if (parser.CurrentConditionalDirectiveState
                || !(typeRef != null && typeRef.IsConst && typeRef.Reference is bool && (bool)typeRef.Reference))
                SkipSection(parser);
            else
                parser.CurrentConditionalDirectiveState = _isActive = true;
        }

        #endregion

        #region /* RENDERING */

        protected override void AsTextArgument(CodeWriter writer, RenderFlags flags)
        {
            if (_expression != null)
                _expression.AsText(writer, flags);
        }

        #endregion
    }
}
