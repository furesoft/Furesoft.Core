using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
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