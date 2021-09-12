using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.Resolving;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base
{
    /// <summary>
    /// The common base class of the <see cref="TypeOf"/>, <see cref="SizeOf"/>, and <see cref="DefaultValue"/> operators.
    /// </summary>
    public abstract class TypeOperator : SingleArgumentOperator
    {
        #region /* CONSTRUCTORS */

        /// <summary>
        /// Create a type operator - the expression must evaluate to a <see cref="TypeRef"/> in valid code.
        /// </summary>
        protected TypeOperator(Expression type)
            : base(type)
        { }

        #endregion

        #region /* PARSING */

        protected TypeOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion

        #region /* RESOLVING */

        /// <summary>
        /// Resolve all child symbolic references, using the specified <see cref="ResolveCategory"/> and <see cref="ResolveFlags"/>.
        /// </summary>
        public override CodeObject Resolve(ResolveCategory resolveCategory, ResolveFlags flags)
        {
            if (_expression != null)
                _expression = (Expression)_expression.Resolve(ResolveCategory.Type, flags);
            return this;
        }

        #endregion
    }
}
