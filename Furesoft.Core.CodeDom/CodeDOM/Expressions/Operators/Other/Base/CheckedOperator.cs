using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Other.Base
{
    /// <summary>
    /// The common base class of the <see cref="Checked"/> and <see cref="Unchecked"/> operators.
    /// </summary>
    public abstract class CheckedOperator : SingleArgumentOperator
    {
        protected CheckedOperator(Expression expression)
            : base(expression)
        { }

        protected CheckedOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}
