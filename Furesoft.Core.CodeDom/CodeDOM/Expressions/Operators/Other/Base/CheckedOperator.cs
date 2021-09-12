using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base
{
    /// <summary>
    /// The common base class of the <see cref="Checked"/> and <see cref="Unchecked"/> operators.
    /// </summary>
    public abstract class CheckedOperator : SingleArgumentOperator
    {
        #region /* CONSTRUCTORS */

        protected CheckedOperator(Expression expression)
            : base(expression)
        { }

        #endregion

        #region /* PARSING */

        protected CheckedOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
