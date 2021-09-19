using Furesoft.Core.CodeDom.Parsing;
using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Nova.CodeDOM;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base
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
