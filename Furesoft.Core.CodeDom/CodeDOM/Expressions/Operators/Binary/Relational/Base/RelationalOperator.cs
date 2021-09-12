using Furesoft.Core.CodeDom.CodeDOM.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Base;
using Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Base;
using Furesoft.Core.CodeDom.Parsing;

namespace Furesoft.Core.CodeDom.CodeDOM.Expressions.Operators.Binary.Relational.Base
{
    /// <summary>
    /// The common base class of all relational operators (<see cref="Equal"/>, <see cref="NotEqual"/>,
    /// <see cref="GreaterThan"/>, <see cref="LessThan"/>, <see cref="GreaterThanEqual"/>, <see cref="LessThanEqual"/>).
    /// </summary>
    public abstract class RelationalOperator : BinaryBooleanOperator
    {
        #region /* CONSTRUCTORS */

        protected RelationalOperator(Expression left, Expression right)
            : base(left, right)
        { }

        #endregion

        #region /* PARSING */

        protected RelationalOperator(Parser parser, CodeObject parent)
            : base(parser, parent)
        { }

        #endregion
    }
}
