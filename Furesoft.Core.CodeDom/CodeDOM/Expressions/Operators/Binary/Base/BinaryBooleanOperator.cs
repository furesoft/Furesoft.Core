using Furesoft.Core.CodeDom.Parsing;

namespace Nova.CodeDOM
{
    /// <summary>
    /// The common base class of all binary operators that evaluate to boolean values (<see cref="RelationalOperator"/>
    /// [common base of <see cref="Equal"/>, <see cref="NotEqual"/>, <see cref="GreaterThan"/>, <see cref="LessThan"/>,
    /// <see cref="GreaterThanEqual"/>, <see cref="LessThanEqual"/>], <see cref="And"/>, <see cref="Or"/>, <see cref="Is"/>).
    /// </summary>
    public abstract class BinaryBooleanOperator : BinaryOperator
    {
        protected BinaryBooleanOperator(Expression left, Expression right)
            : base(left, right)
        { }

        protected BinaryBooleanOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }
    }
}