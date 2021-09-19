using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all relational operators (<see cref="Equal"/>, <see cref="NotEqual"/>,
    /// <see cref="GreaterThan"/>, <see cref="LessThan"/>, <see cref="GreaterThanEqual"/>, <see cref="LessThanEqual"/>).
    /// </summary>
    public abstract class RelationalOperator : BinaryBooleanOperator
    {
        protected RelationalOperator(Expression left, Expression right)
            : base(left, right)
        { }

        protected RelationalOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}